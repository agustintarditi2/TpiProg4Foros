using MyApp.Application.Models;
using MyApp.Application.Services;
using MyApp.Domain.Entities;
using MyApp.Domain.Enums;
using MyApp.Domain.Exceptions;
using MyApp.Application.Interfaces;
using TpiProg4Foros.Tests.Support;
using Xunit;

namespace TpiProg4Foros.Tests.Application;

public class BanServiceTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;
    private readonly FakeUsers _users = new();
    private readonly IBanRepository _repository;
    private readonly BanRepositoryProxy _bans;

    public BanServiceTests()
    {
        (_repository, _bans) = BanTestSupport.CreateRepository();
    }

    private BanServices Service => new(_repository, _users);

    private (User Target, User Admin) SeedUsers()
    {
        var target = Sample.User("target@example.com");
        var admin = Sample.User("admin@example.com");
        admin.SetRole(UserRole.Admin);
        _users.Items[target.Id.Value] = target;
        _users.Items[admin.Id.Value] = admin;
        return (target, admin);
    }

    [Fact]
    public async Task Admin_can_create_ban_for_existing_user()
    {
        var (target, admin) = SeedUsers();

        var id = await Service.CreateBan(
            new CreateBanDTO(target.Id.Value, 5, "  Spam  "), admin.Id.Value, Ct);

        var ban = Assert.Single(_bans.Items.Values);
        Assert.Equal(id, ban.Id);
        Assert.Equal(target.Id, ban.UserId);
        Assert.Equal(admin.Id, ban.AdminId);
        Assert.Equal(5, ban.Duration);
        Assert.Equal("Spam", ban.Reason);
        Assert.Equal(BanStatus.Activo, ban.Status);
        Assert.Equal(1, _bans.Adds);
    }

    [Fact]
    public async Task Cannot_create_second_active_ban_for_same_user()
    {
        var (target, admin) = SeedUsers();
        var request = new CreateBanDTO(target.Id.Value, 5, "Spam");

        await Service.CreateBan(request, admin.Id.Value, Ct);

        await Assert.ThrowsAsync<DomainException>(() =>
            Service.CreateBan(request, admin.Id.Value, Ct));

        Assert.Single(_bans.Items);
        Assert.Equal(1, _bans.Adds);
    }

    [Fact]
    public async Task Cannot_ban_nonexistent_user()
    {
        var (_, admin) = SeedUsers();

        await Assert.ThrowsAsync<NotFoundException>(() => Service.CreateBan(
            new CreateBanDTO(Guid.NewGuid(), 3, "Reason"), admin.Id.Value, Ct));

        Assert.Empty(_bans.Items);
        Assert.Equal(0, _bans.Adds);
    }

    [Fact]
    public async Task Cannot_create_ban_when_administrator_is_missing()
    {
        var (target, _) = SeedUsers();

        await Assert.ThrowsAsync<NotFoundException>(() => Service.CreateBan(
            new CreateBanDTO(target.Id.Value, 3, "Reason"), Guid.NewGuid(), Ct));

        Assert.Empty(_bans.Items);
    }

    [Theory]
    [InlineData(0, "Reason")]
    [InlineData(-2, "Reason")]
    [InlineData(2, "  ")]
    public async Task Invalid_ban_request_is_not_persisted(int days, string reason)
    {
        var (target, admin) = SeedUsers();

        await Assert.ThrowsAsync<DomainException>(() => Service.CreateBan(
            new CreateBanDTO(target.Id.Value, days, reason), admin.Id.Value, Ct));

        Assert.Empty(_bans.Items);
    }

    [Fact]
    public async Task Existing_bans_can_be_queried()
    {
        var (target, admin) = SeedUsers();
        var ban = new Ban(target, admin, 2, "Reason");
        _bans.Items[ban.Id.Value] = ban;

        var byId = await Service.GetById(ban.Id.Value, Ct);
        var all = await Service.Get(Ct);

        Assert.Equal(ban.Id.Value, byId.Id);
        Assert.Equal(target.Id.Value, byId.UserId);
        Assert.Equal("Activo", byId.Status);
        Assert.Single(all);
    }

    [Fact]
    public async Task Missing_bans_return_not_found()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => Service.GetById(Guid.NewGuid(), Ct));
        await Assert.ThrowsAsync<NotFoundException>(() => Service.Get(Ct));
        await Assert.ThrowsAsync<NotFoundException>(() => Service.Desban(Guid.NewGuid(), Ct));
        await Assert.ThrowsAsync<NotFoundException>(() => Service.DeleteBan(Guid.NewGuid(), Ct));
    }

    [Fact]
    public async Task Modifying_ban_updates_its_duration_reason_and_persistence()
    {
        var (target, admin) = SeedUsers();
        var ban = new Ban(target, admin, 2, "Original");
        _bans.Items[ban.Id.Value] = ban;

        await Service.ModifyBan(new ModifyBanDTO(ban.Id.Value, 6, "  Updated  "), Ct);

        Assert.Equal(6, ban.Duration);
        Assert.Equal("Updated", ban.Reason);
        Assert.Equal(1, _bans.Updates);
    }

    [Fact]
    public async Task Revoking_ban_updates_status_and_persistence()
    {
        var (target, admin) = SeedUsers();
        var ban = new Ban(target, admin, 2, "Reason");
        _bans.Items[ban.Id.Value] = ban;

        await Service.Desban(ban.Id.Value, Ct);

        Assert.Equal(BanStatus.Revocado, ban.Status);
        Assert.Equal(1, _bans.Updates);
    }

    [Fact]
    public async Task Reading_expired_ban_refreshes_persisted_status()
    {
        var (target, admin) = SeedUsers();
        var ban = new Ban(target, admin, 2, "Reason");
        typeof(Ban).GetProperty(nameof(Ban.EndDate))!
            .SetValue(ban, DateTime.Today.AddDays(-1));
        _bans.Items[ban.Id.Value] = ban;

        var result = await Service.GetById(ban.Id.Value, Ct);

        Assert.Equal("Vencido", result.Status);
        Assert.Equal(BanStatus.Vencido, ban.Status);
        Assert.Equal(1, _bans.Updates);
    }

    [Fact]
    public async Task Cannot_revoke_same_ban_twice()
    {
        var (target, admin) = SeedUsers();
        var ban = new Ban(target, admin, 2, "Reason");
        _bans.Items[ban.Id.Value] = ban;

        await Service.Desban(ban.Id.Value, Ct);

        await Assert.ThrowsAsync<DomainException>(() =>
            Service.Desban(ban.Id.Value, Ct));

        Assert.Equal(BanStatus.Revocado, ban.Status);
        Assert.Equal(1, _bans.Updates);
    }

    [Fact]
    public async Task Deleting_ban_removes_record()
    {
        var (target, admin) = SeedUsers();
        var ban = new Ban(target, admin, 2, "Reason");
        _bans.Items[ban.Id.Value] = ban;

        await Service.DeleteBan(ban.Id.Value, Ct);

        Assert.Empty(_bans.Items);
        Assert.Equal(1, _bans.Deletes);
    }
}
