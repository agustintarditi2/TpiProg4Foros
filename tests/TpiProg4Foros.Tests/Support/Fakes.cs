using System.Security.Claims;
using MyApp.Application.Abstractions;
using MyApp.Application.Interfaces;
using MyApp.Domain.Entities;
using MyApp.Domain.Enums;
using MyApp.Domain.ValueObjects;

namespace TpiProg4Foros.Tests.Support;

// All fakes are isolated per test: no database, clock or external services required.
internal static class Sample
{
    public static readonly DateTimeOffset Now = new(2026, 11, 11, 12, 0, 0, TimeSpan.Zero);

    public static User User(string email = "user@example.com") =>
        new("Example User", new DateOnly(2000, 5, 10), email, "valid-password-hash", Now);

    public static Forum Forum() => new("General", "General discussion");

    public static Post Post(User user, Forum forum, string body = "Hello") =>
        new(body, user.Id, forum.Id, Now);
}

internal sealed class FakeClock : IClock
{
    public DateTimeOffset UtcNow { get; set; } = Sample.Now;
}

internal sealed class FakeHasher : IPasswordHasher
{
    public string Hash(string plainPassword) => "hashed-" + plainPassword;
    public bool Verify(string plainPassword, string hash) => Hash(plainPassword) == hash;
}

internal sealed class FakeSanitizer : ISanitizer
{
    public string Sanitize(string html) => html.Trim();
}

internal sealed class FakeJwt : IJwtServices
{
    public UserId LastUser { get; private set; }
    public UserRole LastRole { get; private set; }
    public ClaimsPrincipal Principal { get; set; } = new(new ClaimsIdentity());

    public string GenerateToken(UserId userId, UserRole role)
    {
        LastUser = userId;
        LastRole = role;
        return "test-token";
    }

    public ClaimsPrincipal ValidateToken(string token) => Principal;

    public void SetUserClaim(string? value) =>
        Principal = new ClaimsPrincipal(new ClaimsIdentity(
            value is null ? [] : [new Claim("user", value)], "test"));
}

internal sealed class FakeForums : IForumRepository
{
    public Dictionary<Guid, Forum> Items { get; } = [];
    public int Updates { get; private set; }
    public int Deletes { get; private set; }

    public Task<List<Forum>> Get(CancellationToken ct) =>
        Task.FromResult(Items.Values.ToList());

    public Task<Forum?> GetById(ForumId id, CancellationToken ct) =>
        Task.FromResult(Items.GetValueOrDefault(id.Value));

    public Task<Forum> Add(Forum entity, CancellationToken ct)
    {
        Items[entity.Id.Value] = entity;
        return Task.FromResult(entity);
    }

    public Task Update(Forum entity, CancellationToken ct)
    {
        Updates++;
        Items[entity.Id.Value] = entity;
        return Task.CompletedTask;
    }

    public Task Delete(Forum entity, CancellationToken ct)
    {
        Deletes++;
        Items.Remove(entity.Id.Value);
        return Task.CompletedTask;
    }
}

internal sealed class FakePosts : IPostRepository
{
    public Dictionary<Guid, Post> Items { get; } = [];
    public int Updates { get; private set; }
    public int Deletes { get; private set; }

    public Task<List<Post>> GetByForum(ForumId id, CancellationToken ct) =>
        Task.FromResult(Items.Values.Where(p => p.ForumId == id).ToList());

    public Task<List<Post>> GetByPoster(UserId id, CancellationToken ct) =>
        Task.FromResult(Items.Values.Where(p => p.Poster == id).ToList());

    public Task<Post?> GetById(PostId id, CancellationToken ct) =>
        Task.FromResult(Items.GetValueOrDefault(id.Value));

    public Task<Post> Add(Post entity, CancellationToken ct)
    {
        Items[entity.Id.Value] = entity;
        return Task.FromResult(entity);
    }

    public Task Update(Post entity, CancellationToken ct)
    {
        Updates++;
        Items[entity.Id.Value] = entity;
        return Task.CompletedTask;
    }

    public Task Delete(Post entity, CancellationToken ct)
    {
        Deletes++;
        Items.Remove(entity.Id.Value);
        return Task.CompletedTask;
    }
}

internal sealed class FakeUsers : IUserRepository
{
    public Dictionary<Guid, User> Items { get; } = [];
    public int Updates { get; private set; }

    public Task<List<User>> Get(CancellationToken ct) =>
        Task.FromResult(Items.Values.ToList());

    public Task<User?> GetById(UserId id, CancellationToken ct) =>
        Task.FromResult(Items.GetValueOrDefault(id.Value));

    public Task<User?> GetByEmail(EmailAddress email, CancellationToken ct) =>
        Task.FromResult(Items.Values.FirstOrDefault(u => u.Email == email));

    public Task<User> Add(User entity, CancellationToken ct)
    {
        Items[entity.Id.Value] = entity;
        return Task.FromResult(entity);
    }

    public Task Delete(User entity, CancellationToken ct)
    {
        Items.Remove(entity.Id.Value);
        return Task.CompletedTask;
    }

    public Task Update(User entity, CancellationToken ct)
    {
        Updates++;
        Items[entity.Id.Value] = entity;
        return Task.CompletedTask;
    }

    public Task<bool> EmailExists(EmailAddress email) =>
        Task.FromResult(Items.Values.Any(u => u.Email == email));
}
