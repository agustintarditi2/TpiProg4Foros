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
// Reflection allows this test-only file to compile before GetActiveByUserId
// is merged into the Tests branch.
public class BanRepositoryTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;

    private static async Task<List<Ban>> GetActiveByUserId(
        BanRepository repository, UserId userId)
    {
        var method = typeof(BanRepository).GetMethod(
            "GetActiveByUserId", [typeof(UserId), typeof(CancellationToken)]);
        Assert.NotNull(method);

        var pending = method.Invoke(repository, [userId, Ct]);
        var task = Assert.IsAssignableFrom<Task<List<Ban>>>(pending);
        return await task;
    }

    [Fact]
    public async Task Query_returns_active_bans_only_for_requested_user()
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

        Assert.Single(await GetActiveByUserId(repository, target.Id));
        Assert.Empty(await GetActiveByUserId(repository, other.Id));

        ban.Desban();
        await db.SaveChangesAsync(Ct);

        Assert.Empty(await GetActiveByUserId(repository, target.Id));
    }
}
