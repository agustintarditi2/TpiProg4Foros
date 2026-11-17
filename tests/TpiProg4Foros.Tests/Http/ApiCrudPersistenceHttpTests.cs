using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyApp.Domain.Entities;
using MyApp.Domain.ValueObjects;
using MyApp.Infrastructure.Persistence;
using Xunit;

namespace TpiProg4Foros.Tests.Http;

// Real HTTP pipeline, JWT, service/repository DI, and isolated SQLite.
// Each test uses a separate factory and database; Railway and local DB files are untouched.
public sealed class ApiCrudPersistenceHttpTests
{
    private static async Task LoginAsAsync(HttpClient client, string email)
    {
        using var response = await client.PostAsJsonAsync("/api/auth/login",
            new { Email = email, PlainPassword = ApiTestFactory.TestPassword });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var jwt = (await response.Content.ReadAsStringAsync()).Trim();
        Assert.Equal(3, jwt.Split('.').Length);
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", jwt);
    }

    private static async Task<Guid> ReadIdAsync(HttpResponseMessage response)
    {
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.GetProperty("id").GetGuid();
    }

    [Fact]
    public async Task Owner_deletes_post_and_separate_query_confirms_database_deletion()
    {
        using var factory = new ApiTestFactory();
        var seed = await factory.SeedAsync();
        using var client = factory.CreateHttpsClient();
        await LoginAsAsync(client, "http-user@example.invalid");

        using var deleted = await client.DeleteAsync($"/Post/{seed.PostId}");
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);

        using var retrieved = await client.GetAsync($"/Post/{seed.PostId}");
        Assert.Equal(HttpStatusCode.NotFound, retrieved.StatusCode);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.False(await db.Posts.AnyAsync(p => p.Id == (PostId)seed.PostId));
    }

    [Fact]
    public async Task Owner_hides_and_restores_post_across_http_requests()
    {
        using var factory = new ApiTestFactory();
        var seed = await factory.SeedAsync();
        using var client = factory.CreateHttpsClient();
        await LoginAsAsync(client, "http-user@example.invalid");

        using var hidden = await client.PatchAsJsonAsync($"/Post/{seed.PostId}/visible", true);
        Assert.Equal(HttpStatusCode.NoContent, hidden.StatusCode);
        using var invisible = await client.GetAsync($"/Post/{seed.PostId}");
        Assert.Equal(HttpStatusCode.NotFound, invisible.StatusCode);

        using var restored = await client.PatchAsJsonAsync($"/Post/{seed.PostId}/visible", false);
        Assert.Equal(HttpStatusCode.NoContent, restored.StatusCode);
        using var visible = await client.GetAsync($"/Post/{seed.PostId}");
        Assert.Equal(HttpStatusCode.OK, visible.StatusCode);
        using var json = JsonDocument.Parse(await visible.Content.ReadAsStringAsync());
        Assert.Equal("A real HTTP post", json.RootElement.GetProperty("body").GetString());
    }

    [Fact]
    public async Task Comment_edit_is_persisted_between_independent_requests()
    {
        using var factory = new ApiTestFactory();
        var seed = await factory.SeedAsync();
        using var client = factory.CreateHttpsClient();
        await LoginAsAsync(client, "http-user@example.invalid");

        using var created = await client.PostAsJsonAsync("/Comment",
            new { ParentPost = seed.PostId, Body = "Original comment" });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var commentId = await ReadIdAsync(created);

        using var changed = await client.PatchAsJsonAsync($"/Comment/{commentId}", "Edited comment");
        Assert.Equal(HttpStatusCode.NoContent, changed.StatusCode);

        using var retrieved = await client.GetAsync($"/Comment/{commentId}");
        Assert.Equal(HttpStatusCode.OK, retrieved.StatusCode);
        using var json = JsonDocument.Parse(await retrieved.Content.ReadAsStringAsync());
        Assert.Equal("Edited comment", json.RootElement.GetProperty("body").GetString());
        Assert.True(json.RootElement.GetProperty("edited").GetBoolean());
    }

    [Fact]
    public async Task Comment_visibility_changes_are_persisted_and_reversible()
    {
        using var factory = new ApiTestFactory();
        var seed = await factory.SeedAsync();
        using var client = factory.CreateHttpsClient();
        await LoginAsAsync(client, "http-user@example.invalid");

        using var created = await client.PostAsJsonAsync("/Comment",
            new { ParentPost = seed.PostId, Body = "Private comment" });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var id = await ReadIdAsync(created);

        using var hidden = await client.PatchAsJsonAsync($"/Comment/{id}/visible", true);
        Assert.Equal(HttpStatusCode.NoContent, hidden.StatusCode);
        using var retrievedHidden = await client.GetAsync($"/Comment/{id}");
        Assert.Equal(HttpStatusCode.OK, retrievedHidden.StatusCode);
        using (var json = JsonDocument.Parse(await retrievedHidden.Content.ReadAsStringAsync()))
        {
            Assert.True(json.RootElement.GetProperty("invisible").GetBoolean());
            Assert.NotEqual("Private comment", json.RootElement.GetProperty("body").GetString());
        }

        using var restored = await client.PatchAsJsonAsync($"/Comment/{id}/visible", false);
        Assert.Equal(HttpStatusCode.NoContent, restored.StatusCode);
        using var retrieved = await client.GetAsync($"/Comment/{id}");
        Assert.Equal(HttpStatusCode.OK, retrieved.StatusCode);
        using var restoredJson = JsonDocument.Parse(await retrieved.Content.ReadAsStringAsync());
        Assert.False(restoredJson.RootElement.GetProperty("invisible").GetBoolean());
        Assert.Equal("Private comment", restoredJson.RootElement.GetProperty("body").GetString());
    }

    [Fact]
    public async Task Forum_update_and_deactivation_survive_new_requests()
    {
        using var factory = new ApiTestFactory();
        await factory.SeedAsync();
        using var client = factory.CreateHttpsClient();
        await LoginAsAsync(client, "http-admin@example.invalid");

        using var created = await client.PostAsJsonAsync("/Forum",
            new { Title = "Created Forum", Description = "New description" });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var id = await ReadIdAsync(created);

        using var updated = await client.PutAsJsonAsync($"/Forum/{id}",
            new { Title = "Edited Forum", Description = "Edited description" });
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);

        using var disabled = await client.PatchAsJsonAsync($"/Forum/{id}/state", false);
        Assert.Equal(HttpStatusCode.NoContent, disabled.StatusCode);

        using var retrieved = await client.GetAsync($"/Forum/{id}");
        Assert.Equal(HttpStatusCode.OK, retrieved.StatusCode);
        using var json = JsonDocument.Parse(await retrieved.Content.ReadAsStringAsync());
        Assert.Equal("Edited Forum", json.RootElement.GetProperty("title").GetString());
        Assert.Equal("Edited description", json.RootElement.GetProperty("description").GetString());
        Assert.False(json.RootElement.GetProperty("state").GetBoolean());
    }

    [Fact]
    public async Task Ban_modification_and_revocation_are_persisted()
    {
        using var factory = new ApiTestFactory();
        var seed = await factory.SeedAsync();
        Guid banId = Guid.Empty;
        await factory.ModifyDbAsync(db =>
        {
            var target = db.Users.Single(u => u.Id == (UserId)seed.OtherId);
            var admin = db.Users.Single(u => u.Id == (UserId)seed.AdminId);
            var ban = new Ban(target, admin, 5, "Original reason");
            banId = ban.Id.Value;
            db.Bans.Add(ban);
        });
        using var client = factory.CreateHttpsClient();
        await LoginAsAsync(client, "http-admin@example.invalid");

        using var updated = await client.PatchAsJsonAsync("/Ban",
            new { Id = banId, NewDays = 7, NewReason = "Updated reason" });
        Assert.Equal(HttpStatusCode.NoContent, updated.StatusCode);

        using var modified = await client.GetAsync($"/Ban/{banId}");
        Assert.Equal(HttpStatusCode.OK, modified.StatusCode);
        using (var json = JsonDocument.Parse(await modified.Content.ReadAsStringAsync()))
        {
            Assert.Equal(7, json.RootElement.GetProperty("duration").GetInt32());
            Assert.Equal("Updated reason", json.RootElement.GetProperty("reason").GetString());
        }

        using var revoked = await client.PatchAsJsonAsync($"/Ban/{banId}/revoke", (object?)null);
        Assert.Equal(HttpStatusCode.NoContent, revoked.StatusCode);
        using var retrieved = await client.GetAsync($"/Ban/{banId}");
        Assert.Equal(HttpStatusCode.OK, retrieved.StatusCode);
        using var status = JsonDocument.Parse(await retrieved.Content.ReadAsStringAsync());
        Assert.Equal("Revocado", status.RootElement.GetProperty("status").GetString());
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("http-user@example.invalid", HttpStatusCode.Forbidden)]
    [InlineData("http-admin@example.invalid", HttpStatusCode.OK)]
    [InlineData("http-sysadmin@example.invalid", HttpStatusCode.OK)]
    public async Task Full_user_endpoint_requires_correct_admin_role(
        string? actorEmail, HttpStatusCode expected)
    {
        using var factory = new ApiTestFactory();
        var seed = await factory.SeedAsync();
        using var client = factory.CreateHttpsClient();
        if (actorEmail is not null)
            await LoginAsAsync(client, actorEmail);

        using var response = await client.GetAsync($"/User/{seed.UserId}/full");
        Assert.Equal(expected, response.StatusCode);
    }
}
