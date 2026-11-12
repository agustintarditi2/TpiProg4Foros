using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Entities;
using MyApp.Domain.ValueObjects;
using MyApp.Infrastructure.Persistence;
using MyApp.Infrastructure.Persistence.Repositories;
using TpiProg4Foros.Tests.Support;
using Xunit;

namespace TpiProg4Foros.Tests.Infrastructure;

// Uses an isolated in-memory SQLite database, never ForosTup.db.
// Reflection allows this test-only file to compile before HasActiveBan
// is merged from the user's local production changes.
public class BanRepositoryTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    private static async Task<bool> HasActiveBan(
        BanRepository repository, UserId userId, DateTime date)
    {
        var method = typeof(BanRepository).GetMethod(
            "HasActiveBan", [typeof(UserId), typeof(DateTime), typeof(CancellationToken)]);
        Assert.NotNull(method);

        var pending = method.Invoke(repository, [userId, date, Ct]);
        var task = Assert.IsAssignableFrom<Task<bool>>(pending);
        return await task;
    }

    [Fact]
    public async Task Query_blocks_only_target_with_active_ban_within_validity_dates()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(Ct);
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new ApplicationDbContext(options);
        await db.Database.EnsureCreatedAsync(Ct);

        var target = Sample.User("target@example.com");
        var admin = Sample.User("admin@example.com");
        admin.SetRole(MyApp.Domain.Enums.UserRole.Admin);
        var other = Sample.User("other@example.com");
        db.Users.AddRange(target, admin, other);
        await db.SaveChangesAsync(Ct);

        var ban = new Ban(target, admin, 7, "Reason");
        db.Bans.Add(ban);
        await db.SaveChangesAsync(Ct);

        var repository = new BanRepository(db);
        var today = ban.StartDate;

        Assert.True(await HasActiveBan(repository, target.Id, today));
        Assert.False(await HasActiveBan(repository, other.Id, today));
        Assert.False(await HasActiveBan(repository, target.Id, ban.EndDate));

        ban.Desban();
        await db.SaveChangesAsync(Ct);

        Assert.False(await HasActiveBan(repository, target.Id, today));
    }
}
