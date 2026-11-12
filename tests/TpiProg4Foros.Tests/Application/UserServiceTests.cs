using MyApp.Application.Models;
using MyApp.Application.Services;
using MyApp.Domain.Enums;
using MyApp.Domain.Exceptions;
using TpiProg4Foros.Tests.Support;
using Xunit;

namespace TpiProg4Foros.Tests.Application;

public class UserServiceTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;
    private readonly FakeUsers _users = new();
    private readonly FakeHasher _hasher = new();
    private readonly FakeClock _clock = new();

    private UserServices Service => new(_hasher, _users, _clock);

    [Fact]
    public async Task Registration_hashes_password_and_creates_default_role()
    {
        var id = await Service.CreateUser(
            new CreateUserDTO("Ana", new DateOnly(2000, 1, 1),
                "ANA@example.com", "Secret123!"), Ct);

        var user = _users.Items[id.Value];
        Assert.Equal("ana@example.com", user.Email.Value);
        Assert.Equal(_hasher.Hash("Secret123!"), user.Password.Value);
        Assert.Equal(UserRole.User, user.Role);
    }

    [Fact]
    public async Task Duplicate_email_is_rejected_before_registration()
    {
        var existing = Sample.User();
        _users.Items[existing.Id.Value] = existing;

        await Assert.ThrowsAsync<DomainException>(() => Service.CreateUser(
            new CreateUserDTO("New", new DateOnly(2001, 1, 1),
                "USER@EXAMPLE.COM", "Secret123!"), Ct));
        Assert.Single(_users.Items);
    }

    [Fact]
    public async Task Update_modifies_only_authenticated_account()
    {
        var mine = Sample.User();
        var other = Sample.User("other@example.com");
        _users.Items[mine.Id.Value] = mine;
        _users.Items[other.Id.Value] = other;

        await Service.UpdateUser(new PatchUserDTO("New name", null, "Secret123!"),
            mine.Id.Value, Ct);

        Assert.Equal("New name", mine.Name);
        Assert.Equal(_hasher.Hash("Secret123!"), mine.Password.Value);
        Assert.Equal("Example User", other.Name);
        Assert.Equal(1, _users.Updates);
    }

    [Fact]
    public async Task Empty_patch_is_no_op()
    {
        var mine = Sample.User();
        _users.Items[mine.Id.Value] = mine;

        await Service.UpdateUser(new PatchUserDTO(null, null, null), mine.Id.Value, Ct);

        Assert.Equal(0, _users.Updates);
    }

    [Fact]
    public async Task Duplicate_email_patch_is_rejected()
    {
        var mine = Sample.User();
        var other = Sample.User("other@example.com");
        _users.Items[mine.Id.Value] = mine;
        _users.Items[other.Id.Value] = other;

        await Assert.ThrowsAsync<DomainException>(() => Service.UpdateUser(
            new PatchUserDTO(null, "other@example.com", null), mine.Id.Value, Ct));
        Assert.Equal("user@example.com", mine.Email.Value);
        Assert.Equal(0, _users.Updates);
    }

    [Fact]
    public async Task Invalid_name_patch_does_not_persist()
    {
        var mine = Sample.User();
        _users.Items[mine.Id.Value] = mine;

        await Assert.ThrowsAsync<DomainException>(() => Service.UpdateUser(
            new PatchUserDTO(" ", null, null), mine.Id.Value, Ct));

        Assert.Equal("Example User", mine.Name);
        Assert.Equal(0, _users.Updates);
    }

    [Fact]
    public async Task Non_sysadmin_cannot_change_roles()
    {
        var admin = Sample.User();
        admin.SetRole(UserRole.Admin);
        var target = Sample.User("target@example.com");
        _users.Items[admin.Id.Value] = admin;
        _users.Items[target.Id.Value] = target;

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            Service.ChangeUserRole(target.Id.Value, (int)UserRole.Admin, admin.Id.Value, Ct));
        Assert.Equal(UserRole.User, target.Role);
    }

    [Fact]
    public async Task Sysadmin_cannot_change_own_role()
    {
        var sysadmin = Sample.User();
        sysadmin.SetRole(UserRole.SysAdmin);
        _users.Items[sysadmin.Id.Value] = sysadmin;

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            Service.ChangeUserRole(sysadmin.Id.Value, (int)UserRole.User, sysadmin.Id.Value, Ct));
        Assert.Equal(UserRole.SysAdmin, sysadmin.Role);
    }

    [Fact]
    public async Task Sysadmin_cannot_assign_unknown_role()
    {
        var sysadmin = Sample.User();
        sysadmin.SetRole(UserRole.SysAdmin);
        var target = Sample.User("target@example.com");
        _users.Items[sysadmin.Id.Value] = sysadmin;
        _users.Items[target.Id.Value] = target;

        await Assert.ThrowsAsync<DomainException>(() =>
            Service.ChangeUserRole(target.Id.Value, 99, sysadmin.Id.Value, Ct));
        Assert.Equal(UserRole.User, target.Role);
        Assert.Equal(0, _users.Updates);
    }

    [Fact]
    public async Task Sysadmin_can_change_other_users_role()
    {
        var sysadmin = Sample.User();
        sysadmin.SetRole(UserRole.SysAdmin);
        var target = Sample.User("target@example.com");
        _users.Items[sysadmin.Id.Value] = sysadmin;
        _users.Items[target.Id.Value] = target;

        await Service.ChangeUserRole(target.Id.Value, (int)UserRole.Admin, sysadmin.Id.Value, Ct);

        Assert.Equal(UserRole.Admin, target.Role);
        Assert.Equal(1, _users.Updates);
    }

    [Fact]
    public async Task Deleting_own_account_deactivates_without_removing_user_record()
    {
        var mine = Sample.User();
        _users.Items[mine.Id.Value] = mine;

        await Service.DeleteOwnAccount(mine.Id.Value, Ct);

        Assert.Single(_users.Items);
        Assert.False(mine.IsActive);
        Assert.Equal(1, _users.Updates);
    }

    [Fact]
    public async Task Missing_user_throws_not_found()
    {
        var id = Guid.NewGuid();
        await Assert.ThrowsAsync<NotFoundException>(() => Service.GetById(id, Ct));
        await Assert.ThrowsAsync<NotFoundException>(() =>
            Service.UpdateUser(new PatchUserDTO("Name", null, null), id, Ct));
        await Assert.ThrowsAsync<NotFoundException>(() => Service.DeleteOwnAccount(id, Ct));
    }

    [Fact]
    public async Task Get_returns_all_users_or_not_found_when_empty()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => Service.Get(Ct));
        var mine = Sample.User();
        _users.Items[mine.Id.Value] = mine;

        var results = await Service.Get(Ct);
        Assert.Single(results);
        Assert.Equal(mine.Id.Value, results[0].UserId);
    }
}
