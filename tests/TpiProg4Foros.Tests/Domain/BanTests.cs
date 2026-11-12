using MyApp.Domain.Entities;
using MyApp.Domain.Enums;
using MyApp.Domain.Exceptions;
using TpiProg4Foros.Tests.Support;
using Xunit;

namespace TpiProg4Foros.Tests.Domain;

public class BanTests
{
    private static (User Target, User Admin) Users() =>
        (Sample.User("target@example.com"), Sample.User("admin@example.com"));

    [Fact]
    public void Creating_ban_sets_target_admin_dates_reason_and_active_status()
    {
        var (target, admin) = Users();
        var ban = new Ban(target, admin, 7, "  Repeated spam  ");

        Assert.NotEqual(Guid.Empty, ban.Id.Value);
        Assert.Equal(target.Id, ban.UserId);
        Assert.Equal(admin.Id, ban.AdminId);
        Assert.Equal(7, ban.Duration);
        Assert.Equal(ban.StartDate.AddDays(7), ban.EndDate);
        Assert.Equal("Repeated spam", ban.Reason);
        Assert.Equal(BanStatus.Activo, ban.Status);
        Assert.Equal(BanStatus.Activo, ban.Status);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-30)]
    public void Ban_duration_must_be_positive(int days)
    {
        var (target, admin) = Users();
        Assert.Throws<DomainException>(() => new Ban(target, admin, days, "Reason"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Ban_reason_is_required(string? reason)
    {
        var (target, admin) = Users();
        Assert.Throws<DomainException>(() => new Ban(target, admin, 2, reason!));
    }

    [Fact]
    public void Extending_ban_changes_end_date_duration_and_reason()
    {
        var (target, admin) = Users();
        var ban = new Ban(target, admin, 2, "Original");

        ban.ModifyBan(7, "  Updated  ");

        Assert.Equal(7, ban.Duration);
        Assert.Equal(ban.StartDate.AddDays(7), ban.EndDate);
        Assert.Equal("Updated", ban.Reason);
        Assert.Equal(BanStatus.Activo, ban.Status);
    }

    [Fact]
    public void Modifying_ban_to_zero_days_is_rejected()
    {
        var (target, admin) = Users();
        var ban = new Ban(target, admin, 5, "Reason");

        Assert.Throws<DomainException>(() => ban.ModifyBan(0, "Invalid duration"));

        Assert.Equal(5, ban.Duration);
        Assert.Equal(BanStatus.Activo, ban.Status);
    }

    [Fact]
    public void Admin_cannot_ban_self()
    {
        var admin = Sample.User("admin@example.com");
        admin.SetRole(UserRole.Admin);

        Assert.Throws<DomainException>(() => new Ban(admin, admin, 2, "Reason"));
    }

    [Fact]
    public void Admin_cannot_ban_sysadmin()
    {
        var target = Sample.User("sysadmin@example.com");
        target.SetRole(UserRole.SysAdmin);
        var admin = Sample.User("admin@example.com");
        admin.SetRole(UserRole.Admin);

        Assert.Throws<UnauthorizedAccessException>(() => new Ban(target, admin, 2, "Reason"));
    }

    [Fact]
    public void Admin_cannot_ban_another_admin()
    {
        var target = Sample.User("other-admin@example.com");
        target.SetRole(UserRole.Admin);
        var admin = Sample.User("admin@example.com");
        admin.SetRole(UserRole.Admin);

        // Regression: the policy says an admin can only ban a lower role.
        Assert.Throws<UnauthorizedAccessException>(() => new Ban(target, admin, 2, "Reason"));
    }

    [Fact]
    public void Revoking_ban_prevents_it_from_being_active()
    {
        var (target, admin) = Users();
        var ban = new Ban(target, admin, 7, "Reason");

        ban.Desban();

        Assert.Equal(BanStatus.Revocado, ban.Status);
        Assert.Equal(BanStatus.Revocado, ban.Status);
    }
}
