using MyApp.Application.Interfaces;
using MyApp.Application.Services;
using MyApp.Domain.Entities;
using MyApp.Domain.Enums;
using TpiProg4Foros.Tests.Support;
using Xunit;

namespace TpiProg4Foros.Tests.Application;

public class BanAccessTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    private static Task EnsureNotBanned(BanServices service, Guid userId)
    {
        // The helper compiles against the older Tests branch; the test fails
        // until BanServices.EnsureNotBanned from develop is included.
        var method = typeof(BanServices).GetMethod("EnsureNotBanned");
        Assert.NotNull(method);
        return Assert.IsAssignableFrom<Task>(method.Invoke(service, [userId, Ct]));
    }

    private static (User Target, Ban Ban) BanUser(BanRepositoryProxy spy)
    {
        var target = Sample.User("target@example.com");
        var admin = Sample.User("admin@example.com");
        admin.SetRole(UserRole.Admin);
        var ban = new Ban(target, admin, 7, "Reason");
        spy.Items[ban.Id.Value] = ban;
        return (target, ban);
    }

    [Fact]
    public async Task Active_ban_rejects_access()
    {
        var (repository, spy) = BanTestSupport.CreateRepository();
        var (target, _) = BanUser(spy);
        var service = new BanServices(repository, new FakeUsers());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => EnsureNotBanned(service, target.Id.Value));
    }

    [Fact]
    public async Task Revoked_ban_allows_access()
    {
        var (repository, spy) = BanTestSupport.CreateRepository();
        var (target, ban) = BanUser(spy);
        ban.Desban();
        var service = new BanServices(repository, new FakeUsers());

        await EnsureNotBanned(service, target.Id.Value);
    }

    [Fact]
    public async Task Expired_ban_allows_access_and_refreshes_stored_status()
    {
        var (repository, spy) = BanTestSupport.CreateRepository();
        var (target, ban) = BanUser(spy);
        typeof(Ban).GetProperty(nameof(Ban.EndDate))!
            .SetValue(ban, DateTime.Today.AddDays(-1));
        var service = new BanServices(repository, new FakeUsers());

        await EnsureNotBanned(service, target.Id.Value);

        Assert.Equal(BanStatus.Vencido, ban.Status);
        Assert.Equal(1, spy.Updates);
    }
}
