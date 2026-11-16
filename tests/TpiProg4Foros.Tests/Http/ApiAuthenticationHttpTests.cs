using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyApp.Domain.Entities;
using MyApp.Domain.Enums;
using MyApp.Infrastructure.Persistence;
using Xunit;

namespace TpiProg4Foros.Tests.Http;

// End-to-end within ASP.NET Core's real request pipeline: TestServer,
// real JwtBearer validation, controllers, DI, middleware and SQLite repositories.
public sealed class ApiAuthenticationHttpTests
{
    private static void SetBearer(HttpClient client, string token) =>
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

    private static async Task<string> LoginAsync(HttpClient client, string email)
    {
        using var response = await client.PostAsJsonAsync("/api/auth/login",
            new { Email = email, PlainPassword = ApiTestFactory.TestPassword });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        // AuthController returns Ok(string), which ASP.NET Core formats as text/plain.
        var token = (await response.Content.ReadAsStringAsync()).Trim();
        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.Equal(3, token.Split('.').Length); // signed JWT, not a fake authentication stub
        return token;
    }

    [Fact]
    public async Task Anonymous_post_creation_is_401()
    {
        using var factory = new ApiTestFactory();
        var seed = await factory.SeedAsync();
        using var client = factory.CreateHttpsClient();
        using var response = await client.PostAsJsonAsync("/Post",
            new { Forum = seed.ForumId, Body = "Not authorized" });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Malformed_bearer_token_is_401()
    {
        using var factory = new ApiTestFactory();
        var seed = await factory.SeedAsync();
        using var client = factory.CreateHttpsClient();
        SetBearer(client, "not-a-valid-jwt");
        using var response = await client.PostAsJsonAsync("/Post",
            new { Forum = seed.ForumId, Body = "Not authorized" });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Invalid_password_is_401_with_problem_response()
    {
        using var factory = new ApiTestFactory();
        await factory.SeedAsync();
        using var client = factory.CreateHttpsClient();
        using var response = await client.PostAsJsonAsync("/api/auth/login",
            new { Email = "http-user@example.invalid", PlainPassword = "wrong-password" });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Successful_login_returns_real_jwt_that_can_create_post()
    {
        using var factory = new ApiTestFactory();
        var seed = await factory.SeedAsync();
        using var client = factory.CreateHttpsClient();
        SetBearer(client, await LoginAsync(client, "http-user@example.invalid"));

        using var created = await client.PostAsJsonAsync("/Post",
            new { Forum = seed.ForumId, Body = "Created over HTTP" });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        using var document = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        var id = document.RootElement.GetProperty("id").GetGuid();
        Assert.NotEqual(Guid.Empty, id);

        using var found = await client.GetAsync($"/Post/{id}");
        Assert.Equal(HttpStatusCode.OK, found.StatusCode);
        using var foundJson = JsonDocument.Parse(await found.Content.ReadAsStringAsync());
        Assert.Equal("Created over HTTP", foundJson.RootElement.GetProperty("body").GetString());
    }

    [Fact]
    public async Task Public_post_read_is_200_without_jwt()
    {
        using var factory = new ApiTestFactory();
        var seed = await factory.SeedAsync();
        using var client = factory.CreateHttpsClient();
        using var response = await client.GetAsync($"/Post/{seed.PostId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Missing_post_returns_404()
    {
        using var factory = new ApiTestFactory();
        await factory.SeedAsync();
        using var client = factory.CreateHttpsClient();
        using var response = await client.GetAsync($"/Post/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Blank_post_body_returns_400()
    {
        using var factory = new ApiTestFactory();
        var seed = await factory.SeedAsync();
        using var client = factory.CreateHttpsClient();
        SetBearer(client, await LoginAsync(client, "http-user@example.invalid"));
        using var response = await client.PostAsJsonAsync("/Post",
            new { Forum = seed.ForumId, Body = "  " });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Non_owner_cannot_edit_post_403()
    {
        using var factory = new ApiTestFactory();
        var seed = await factory.SeedAsync();
        using var client = factory.CreateHttpsClient();
        SetBearer(client, await LoginAsync(client, "http-other@example.invalid"));
        using var response = await client.PatchAsJsonAsync($"/Post/{seed.PostId}", "Steal the post");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Owner_can_edit_post_and_change_is_persisted()
    {
        using var factory = new ApiTestFactory();
        var seed = await factory.SeedAsync();
        using var client = factory.CreateHttpsClient();
        SetBearer(client, await LoginAsync(client, "http-user@example.invalid"));
        using var changed = await client.PatchAsJsonAsync($"/Post/{seed.PostId}", "Edited over HTTP");
        Assert.Equal(HttpStatusCode.NoContent, changed.StatusCode);
        using var found = await client.GetAsync($"/Post/{seed.PostId}");
        Assert.Equal(HttpStatusCode.OK, found.StatusCode);
        using var json = JsonDocument.Parse(await found.Content.ReadAsStringAsync());
        Assert.Equal("Edited over HTTP", json.RootElement.GetProperty("body").GetString());
    }

    [Fact]
    public async Task Anonymous_user_cannot_suppress_post_401()
    {
        using var factory = new ApiTestFactory();
        var seed = await factory.SeedAsync();
        using var client = factory.CreateHttpsClient();
        using var response = await client.PatchAsJsonAsync($"/Post/{seed.PostId}/suppress", true);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Ordinary_user_cannot_suppress_post_403()
    {
        using var factory = new ApiTestFactory();
        var seed = await factory.SeedAsync();
        using var client = factory.CreateHttpsClient();
        SetBearer(client, await LoginAsync(client, "http-user@example.invalid"));
        using var response = await client.PatchAsJsonAsync($"/Post/{seed.PostId}/suppress", true);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData("http-admin@example.invalid")]
    [InlineData("http-sysadmin@example.invalid")]
    public async Task Admin_roles_can_suppress_post_and_public_read_becomes_404(string email)
    {
        using var factory = new ApiTestFactory();
        var seed = await factory.SeedAsync();
        using var client = factory.CreateHttpsClient();
        SetBearer(client, await LoginAsync(client, email));
        using var response = await client.PatchAsJsonAsync($"/Post/{seed.PostId}/suppress", true);
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        using var hidden = await client.GetAsync($"/Post/{seed.PostId}");
        Assert.Equal(HttpStatusCode.NotFound, hidden.StatusCode);
    }

    [Fact]
    public async Task Token_is_rejected_after_account_deactivation_401()
    {
        using var factory = new ApiTestFactory();
        var seed = await factory.SeedAsync();
        using var client = factory.CreateHttpsClient();
        var token = await LoginAsync(client, "http-user@example.invalid");

        await factory.ModifyDbAsync(db =>
            db.Users.Single(u => u.Id.Value == seed.UserId)
                .DeactivateAccount("disabled-account-hash"));

        SetBearer(client, token);
        using var response = await client.PostAsJsonAsync("/Post",
            new { Forum = seed.ForumId, Body = "Deactivated user" });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Token_is_rejected_when_account_role_changes_401()
    {
        using var factory = new ApiTestFactory();
        var seed = await factory.SeedAsync();
        using var client = factory.CreateHttpsClient();
        var token = await LoginAsync(client, "http-admin@example.invalid");

        await factory.ModifyDbAsync(db =>
            db.Users.Single(u => u.Id.Value == seed.AdminId).SetRole(UserRole.User));

        SetBearer(client, token);
        using var response = await client.PatchAsJsonAsync($"/Post/{seed.PostId}/suppress", true);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Token_is_rejected_if_user_is_banned_after_login_401()
    {
        using var factory = new ApiTestFactory();
        var seed = await factory.SeedAsync();
        using var client = factory.CreateHttpsClient();
        var token = await LoginAsync(client, "http-user@example.invalid");

        await factory.ModifyDbAsync(db =>
        {
            var user = db.Users.Single(u => u.Id.Value == seed.UserId);
            var admin = db.Users.Single(u => u.Id.Value == seed.AdminId);
            db.Bans.Add(new Ban(user, admin, 5, "HTTP integration test ban"));
        });

        SetBearer(client, token);
        using var response = await client.PostAsJsonAsync("/Post",
            new { Forum = seed.ForumId, Body = "Banned user" });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Valid_jwt_can_create_and_soft_delete_comment()
    {
        using var factory = new ApiTestFactory();
        var seed = await factory.SeedAsync();
        using var client = factory.CreateHttpsClient();
        SetBearer(client, await LoginAsync(client, "http-user@example.invalid"));

        using var created = await client.PostAsJsonAsync("/Comment",
            new { ParentPost = seed.PostId, Body = "A real HTTP comment" });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        using var json = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        var id = json.RootElement.GetProperty("id").GetGuid();

        using var deleted = await client.DeleteAsync($"/Comment/{id}");
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        using var restored = await client.GetAsync($"/Comment/{id}");
        Assert.Equal(HttpStatusCode.OK, restored.StatusCode);
        using var result = JsonDocument.Parse(await restored.Content.ReadAsStringAsync());
        Assert.True(result.RootElement.GetProperty("isDeleted").GetBoolean());
    }

    [Fact]
    public async Task Post_reactions_use_authenticated_identity_and_persist()
    {
        using var factory = new ApiTestFactory();
        var seed = await factory.SeedAsync();
        using var client = factory.CreateHttpsClient();
        SetBearer(client, await LoginAsync(client, "http-user@example.invalid"));

        using var set = await client.PutAsJsonAsync($"/Post/{seed.PostId}/reaction", true);
        Assert.Equal(HttpStatusCode.NoContent, set.StatusCode);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await db.Users.Include(u => u.Reactions)
                .SingleAsync(u => u.Id.Value == seed.UserId);
            Assert.Single(user.Reactions!);
            Assert.Equal(seed.PostId, user.Reactions![0].Post!.Value.Value);
        }

        using var removed = await client.DeleteAsync($"/Post/{seed.PostId}/reaction");
        Assert.Equal(HttpStatusCode.NoContent, removed.StatusCode);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await db.Users.Include(u => u.Reactions)
                .SingleAsync(u => u.Id.Value == seed.UserId);
            Assert.Empty(user.Reactions!);
        }
    }
}
