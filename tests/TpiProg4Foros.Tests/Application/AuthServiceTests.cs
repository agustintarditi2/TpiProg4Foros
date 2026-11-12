using MyApp.Application.Exceptions;
using MyApp.Application.Models;
using MyApp.Application.Services;
using TpiProg4Foros.Tests.Support;
using Xunit;

namespace TpiProg4Foros.Tests.Application;

public class AuthServiceTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;
    private readonly FakeUsers _users = new();
    private readonly FakeHasher _hasher = new();
    private readonly FakeJwt _jwt = new();
    private readonly FakeClock _clock = new();

    private AuthServices Service => new(_users, _hasher, _jwt, _clock);

    [Fact]
    public async Task Correct_credentials_issue_token_with_actual_role()
    {
        var user = Sample.User();
        user.ChangePassword(_hasher.Hash("password123"));
        _users.Items[user.Id.Value] = user;

        var token = await Service.Authenticate(new LoginUserDTO("USER@example.com", "password123"), Ct);

        Assert.Equal("test-token", token);
        Assert.Equal(user.Id, _jwt.LastUser);
        Assert.Equal(user.Role, _jwt.LastRole);
    }

    [Fact]
    public async Task Invalid_credentials_do_not_issue_token()
    {
        var user = Sample.User();
        user.ChangePassword(_hasher.Hash("correct-pass"));
        _users.Items[user.Id.Value] = user;

        await Assert.ThrowsAsync<AuthenticationFailedException>(() =>
            Service.Authenticate(new LoginUserDTO("user@example.com", "wrong-pass"), Ct));
        await Assert.ThrowsAsync<AuthenticationFailedException>(() =>
            Service.Authenticate(new LoginUserDTO("missing@example.com", "correct-pass"), Ct));
    }

    [Fact]
    public async Task Deactivated_account_cannot_log_in_even_with_correct_password()
    {
        var user = Sample.User();
        user.ChangePassword(_hasher.Hash("correct-pass"));
        _users.Items[user.Id.Value] = user;
        user.DeactivateAccount("disabled-password-hash");

        await Assert.ThrowsAsync<AuthenticationFailedException>(() =>
            Service.Authenticate(new LoginUserDTO(user.Email.Value, "correct-pass"), Ct));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("not-a-guid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task Token_missing_or_invalid_user_claim_is_rejected(string? claim)
    {
        _jwt.SetUserClaim(claim);
        await Assert.ThrowsAsync<AuthenticationFailedException>(() =>
            Service.ValidateToken("token", Ct));
    }

    [Fact]
    public async Task Token_for_missing_user_is_rejected()
    {
        _jwt.SetUserClaim(Guid.NewGuid().ToString());
        await Assert.ThrowsAsync<AuthenticationFailedException>(() =>
            Service.ValidateToken("token", Ct));
    }

    [Fact]
    public async Task Token_for_deactivated_user_is_rejected()
    {
        var user = Sample.User();
        _users.Items[user.Id.Value] = user;
        _jwt.SetUserClaim(user.Id.Value.ToString());
        user.DeactivateAccount("disabled-password-hash");

        await Assert.ThrowsAsync<AuthenticationFailedException>(() =>
            Service.ValidateToken("token", Ct));
    }

    [Fact]
    public async Task Token_for_active_user_returns_current_profile()
    {
        var user = Sample.User();
        _users.Items[user.Id.Value] = user;
        _jwt.SetUserClaim(user.Id.Value.ToString());

        var result = await Service.ValidateToken("token", Ct);

        Assert.Equal(user.Id.Value, result.UserId);
        Assert.Equal(user.Name, result.Name);
    }
}
