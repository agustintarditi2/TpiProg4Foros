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
        Assert.True(ban.ValidatState());
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
    public void Shortening_to_today_marks_ban_expired()
    {
        var (target, admin) = Users();
        var ban = new Ban(target, admin, 5, "Reason");

        ban.ModifyBan(0, "Shortened");

        Assert.Equal(BanStatus.Vencido, ban.Status);
        Assert.Equal(ban.StartDate, ban.EndDate);
        Assert.False(ban.ValidatState());
    }

    [Fact]
    public void Revoking_ban_prevents_it_from_being_active()
    {
        var (target, admin) = Users();
        var ban = new Ban(target, admin, 7, "Reason");

        ban.Desban();

        Assert.Equal(BanStatus.Revocado, ban.Status);
        Assert.False(ban.ValidatState());
    }
}
