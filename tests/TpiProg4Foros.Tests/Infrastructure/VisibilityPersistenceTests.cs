using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Entities;
using MyApp.Infrastructure.Persistence;
using TpiProg4Foros.Tests.Support;
using Xunit;

namespace TpiProg4Foros.Tests.Infrastructure;

// Each test uses a separate in-memory SQLite database, not ForosTup.db.
public class VisibilityPersistenceTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task Post_visibility_flags_survive_database_roundtrip(
        bool hiddenByUser, bool suppressedByAdmin)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        var user = Sample.User();
        var forum = Sample.Forum();
        var post = Sample.Post(user, forum);
        post.ToggleHidden(hiddenByUser);
        post.ToggleSuppressed(suppressedByAdmin);

        await using (var db = new ApplicationDbContext(options))
        {
            await db.Database.EnsureCreatedAsync();
            db.Users.Add(user);
            db.Forums.Add(forum);
            db.Posts.Add(post);
            await db.SaveChangesAsync();
        }

        await using (var db = new ApplicationDbContext(options))
        {
            var restored = await db.Posts.SingleAsync(p => p.Id == post.Id);
            Assert.Equal(hiddenByUser, restored.HiddenByUser);
            Assert.Equal(suppressedByAdmin, restored.SuppressedByAdmin);
            Assert.Equal(hiddenByUser || suppressedByAdmin, restored.Invisible);
        }
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task Comment_visibility_flags_survive_database_roundtrip(
        bool hiddenByUser, bool suppressedByAdmin)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        var user = Sample.User();
        var forum = Sample.Forum();
        var post = Sample.Post(user, forum);
        var comment = new Comment("Comment body", post, user, Sample.Now);
        comment.ToggleHidden(hiddenByUser);
        comment.ToggleSuppressed(suppressedByAdmin);

        await using (var db = new ApplicationDbContext(options))
        {
            await db.Database.EnsureCreatedAsync();
            db.Users.Add(user);
            db.Forums.Add(forum);
            db.Posts.Add(post);
            db.Comments.Add(comment);
            await db.SaveChangesAsync();
        }

        await using (var db = new ApplicationDbContext(options))
        {
            var restored = await db.Comments.SingleAsync(c => c.Id == comment.Id);
            Assert.Equal(hiddenByUser, restored.HiddenByUser);
            Assert.Equal(suppressedByAdmin, restored.SuppressedByAdmin);
            Assert.Equal(hiddenByUser || suppressedByAdmin, restored.Invisible);
        }
    }
}
