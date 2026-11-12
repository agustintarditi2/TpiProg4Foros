using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MyApp.Application.Interfaces;
using MyApp.Domain.Entities;
using MyApp.Web.Extensions;
using TpiProg4Foros.Tests.Support;
using Xunit;

namespace TpiProg4Foros.Tests.Web;

// Calls the real registered JwtBearer OnTokenValidated event with isolated dependencies.
// This is a middleware-event test, not a full signed-token HTTP integration test.
public class BanJwtValidationTests
{
    private static User Admin()
    {
        var admin = Sample.User("admin@example.com");
        admin.SetRole(MyApp.Domain.Enums.UserRole.Admin);
        return admin;
    }

    private static void SimulateExpiredBan(Ban ban)
    {
        typeof(Ban).GetProperty(nameof(Ban.EndDate))!
            .SetValue(ban, DateTime.Today.AddDays(-1));
    }

    private static async Task<bool> EventRejects(User user, IBanRepository bans)
    {
        var users = new FakeUsers();
        users.Items[user.Id.Value] = user;

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:SecretForKey"] = new string('K', 64),
                ["Authentication:Issuer"] = "test-issuer",
                ["Authentication:Audience"] = "test-audience"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IUserRepository>(users);
        services.AddSingleton<IBanRepository>(bans);
        services.AddJwtAuthentication(config);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        var httpContext = new DefaultHttpContext { RequestServices = provider };
        var scheme = new AuthenticationScheme(
            JwtBearerDefaults.AuthenticationScheme, null, typeof(JwtBearerHandler));
        var context = new TokenValidatedContext(httpContext, scheme, options)
        {
            Principal = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim("user", user.Id.Value.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            ], JwtBearerDefaults.AuthenticationScheme))
        };

        await options.Events.OnTokenValidated(context);

        return context.Result?.Failure is not null;
    }

    [Fact]
    public async Task Previously_issued_jwt_is_rejected_when_user_becomes_banned()
    {
        var user = Sample.User();
        var (repository, state) = BanTestSupport.CreateRepository();
        var ban = new Ban(user, Admin(), 3, "Reason");
        state.Items[ban.Id.Value] = ban;

        Assert.True(await EventRejects(user, repository));
    }

    [Fact]
    public async Task Jwt_is_accepted_without_active_bans()
    {
        var user = Sample.User();
        var (repository, _) = BanTestSupport.CreateRepository();

        Assert.False(await EventRejects(user, repository));
    }

    [Fact]
    public async Task Previously_issued_jwt_is_accepted_after_revocation()
    {
        var user = Sample.User();
        var (repository, state) = BanTestSupport.CreateRepository();
        var ban = new Ban(user, Admin(), 3, "Reason");
        ban.Desban();
        state.Items[ban.Id.Value] = ban;

        Assert.False(await EventRejects(user, repository));
    }

    [Fact]
    public async Task Previously_issued_jwt_is_accepted_after_expiration()
    {
        var user = Sample.User();
        var (repository, state) = BanTestSupport.CreateRepository();
        var ban = new Ban(user, Admin(), 3, "Reason");
        SimulateExpiredBan(ban);
        state.Items[ban.Id.Value] = ban;

        Assert.False(await EventRejects(user, repository));
    }
}
