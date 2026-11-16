using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MyApp.Domain.Entities;
using MyApp.Domain.Enums;
using MyApp.Infrastructure.Persistence;
using MyApp.Web.Controllers;

namespace TpiProg4Foros.Tests.Http;

// Public controller in the real API assembly is used as the application marker:
// no edits to src/Web/Program.cs (and no Program visibility workaround) are needed.
internal sealed class ApiTestFactory : WebApplicationFactory<PostController>
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    private const string JwtTestKey =
        "HttpIntegrationTestSigningKey_OnlyForTests_2026_32BytesMinimum_1234567890";
    private const string JwtTestIssuer = "https://test-issuer.example.invalid";
    private const string JwtTestAudience = "TpiProg4Foros-Http-Tests";

    public const string TestPassword = "http-integration-password-123";

    public ApiTestFactory()
    {
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Configuration applies to both JWT generation and the real JwtBearer validator.
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:SecretForKey"] = JwtTestKey,
                ["Authentication:Issuer"] = JwtTestIssuer,
                ["Authentication:Audience"] = JwtTestAudience
            });
        });

        builder.ConfigureTestServices(services =>
        {
            // Preserve real EF Core repositories, application services and JWT handling,
            // but replace the file-based connection with an isolated in-memory database.
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<DbContextOptions>();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(_connection));
        });
    }

    public HttpClient CreateHttpsClient() => CreateClient(
        new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });

    public async Task<SeededData> SeedAsync()
    {
        // Force the in-process host to start before accessing DI.
        using var _ = CreateHttpsClient();
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.EnsureCreatedAsync();

        // BCrypt hashes remain compatible with the real password verifier;
        // reduced test cost affects only test-data creation.
        var hash = BCrypt.Net.BCrypt.HashPassword(TestPassword, workFactor: 4);
        User NewUser(string email) => new(
            "HTTP Test User",
            new DateOnly(2000, 1, 1),
            email,
            hash,
            DateTimeOffset.UtcNow);

        var user = NewUser("http-user@example.invalid");
        var other = NewUser("http-other@example.invalid");
        var admin = NewUser("http-admin@example.invalid");
        var sysAdmin = NewUser("http-sysadmin@example.invalid");
        admin.SetRole(UserRole.Admin);
        sysAdmin.SetRole(UserRole.SysAdmin);

        var forum = new Forum("HTTP tests", "An isolated test forum");
        var post = new Post("A real HTTP post", user.Id, forum.Id, DateTimeOffset.UtcNow);

        db.Users.AddRange(user, other, admin, sysAdmin);
        db.Forums.Add(forum);
        db.Posts.Add(post);
        await db.SaveChangesAsync();

        return new SeededData(user.Id.Value, other.Id.Value, admin.Id.Value,
            sysAdmin.Id.Value, forum.Id.Value, post.Id.Value);
    }

    public async Task ModifyDbAsync(Action<ApplicationDbContext> action)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        action(db);
        await db.SaveChangesAsync();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) _connection.Dispose();
    }

    internal record SeededData(
        Guid UserId, Guid OtherId, Guid AdminId, Guid SysAdminId, Guid ForumId, Guid PostId);
}
