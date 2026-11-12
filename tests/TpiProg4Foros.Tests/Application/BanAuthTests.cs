using MyApp.Application.Exceptions;
using MyApp.Application.Interfaces;
using MyApp.Application.Models;
using MyApp.Application.Services;
using MyApp.Domain.Entities;
using TpiProg4Foros.Tests.Support;
using Xunit;

namespace TpiProg4Foros.Tests.Application;

public class BanAuthTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;
    private readonly FakeUsers _users = new();
    private readonly FakeHasher _hasher = new();
    private readonly FakeJwt _jwt = new();
    private readonly FakeClock _clock = new();
    private readonly IBanRepository _repository;
    private readonly BanRepositoryProxy _bans;

    public BanAuthTests()
    {
        (_repository, _bans) = BanTestSupport.CreateRepository();
    }

    private AuthServices Service =>
        BanTestSupport.CreateAuth(_users, _repository, _hasher, _jwt, _clock);

    private User SeedUser()
    {
        var user = Sample.User("banned@example.com");
        user.ChangePassword(_hasher.Hash("correct-pass"));
        _users.Items[user.Id.Value] = user;
        return user;
    }

    private Ban BanUser(User user)
    {
        var admin = Sample.User("admin@example.com");
        admin.SetRole(MyApp.Domain.Enums.UserRole.Admin);
        var ban = new Ban(user, admin, 7, "Violation");
        _bans.Items[ban.Id.Value] = ban;
        return ban;
    }

    private static void SimulateExpiredBan(Ban ban)
    {
        // Test fixture only: creates a historical EndDate without changing production code.
        typeof(Ban).GetProperty(nameof(Ban.EndDate))!
            .SetValue(ban, DateTime.Today.AddDays(-1));
    }

    [Fact]
    public async Task Login_is_rejected_while_ban_is_active_even_with_correct_password()
    {
        var user = SeedUser();
        BanUser(user);

        await Assert.ThrowsAsync<AuthenticationFailedException>(() =>
            Service.Authenticate(new LoginUserDTO(user.Email.Value, "correct-pass"), Ct));
    }

    [Fact]
    public async Task Login_is_allowed_after_ban_is_revoked()
    {
        var user = SeedUser();
        BanUser(user).Desban();

        var token = await Service.Authenticate(
            new LoginUserDTO(user.Email.Value, "correct-pass"), Ct);

        Assert.Equal("test-token", token);
        Assert.Equal(user.Id, _jwt.LastUser);
    }

    [Fact]
    public async Task Login_is_allowed_when_ban_has_expired()
    {
        var user = SeedUser();
        SimulateExpiredBan(BanUser(user));

        var token = await Service.Authenticate(
            new LoginUserDTO(user.Email.Value, "correct-pass"), Ct);

        Assert.Equal("test-token", token);
    }

    [Fact]
    public async Task Explicit_token_validation_rejects_active_ban()
    {
        var user = SeedUser();
        BanUser(user);
        _jwt.SetUserClaim(user.Id.Value.ToString());

        await Assert.ThrowsAsync<AuthenticationFailedException>(() =>
            Service.ValidateToken("previously-issued-token", Ct));
    }

    [Fact]
    public async Task Explicit_token_validation_allows_revoked_ban()
    {
        var user = SeedUser();
        BanUser(user).Desban();
        _jwt.SetUserClaim(user.Id.Value.ToString());

        var profile = await Service.ValidateToken("previously-issued-token", Ct);

        Assert.Equal(user.Id.Value, profile.UserId);
    }

    [Fact]
    public async Task Explicit_token_validation_allows_expired_ban()
    {
        var user = SeedUser();
        SimulateExpiredBan(BanUser(user));
        _jwt.SetUserClaim(user.Id.Value.ToString());

        var profile = await Service.ValidateToken("previously-issued-token", Ct);

        Assert.Equal(user.Id.Value, profile.UserId);
    }
}
