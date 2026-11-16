using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MyApp.Domain.Entities;
using MyApp.Infrastructure.Persistence;
using TpiProg4Foros.Tests.Support;
using Xunit;

namespace TpiProg4Foros.Tests.Infrastructure;

public class ReactionPersistenceTests
{
    private static DbContextOptions<ApplicationDbContext> Options(SqliteConnection connection) =>
        new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options;

    [Fact]
    public async Task Likes_dislikes_and_removals_survive_sqlite_roundtrip()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = Options(connection);
        var author = Sample.User();
        var forum = Sample.Forum();
        var post = Sample.Post(author, forum);
        var comment = new Comment("Comment", post, author, Sample.Now);

        await using (var db = new ApplicationDbContext(options))
        {
            await db.Database.EnsureCreatedAsync();
            db.Users.Add(author);
            db.Forums.Add(forum);
            db.Posts.Add(post);
            db.Comments.Add(comment);
            author.SetPostReaction(post.Id, true);
            author.SetCommentReaction(comment.Id, false);
            await db.SaveChangesAsync();
        }

        await using (var db = new ApplicationDbContext(options))
        {
            var user = await db.Users.Include(u => u.Reactions)
                .SingleAsync(u => u.Id == author.Id);
            Assert.Equal(2, user.Reactions!.Count);
            user.SetPostReaction(post.Id, false);
            user.SetPostReaction(post.Id, false);
            user.RemoveCommentReaction(comment.Id);
            await db.SaveChangesAsync();
        }

        await using (var db = new ApplicationDbContext(options))
        {
            var user = await db.Users.Include(u => u.Reactions)
                .SingleAsync(u => u.Id == author.Id);
            Assert.Single(user.Reactions!);
            Assert.Equal(post.Id, user.Reactions![0].Post);
            Assert.False(user.Reactions![0].Type);

            user.RemovePostReaction(post.Id);
            await db.SaveChangesAsync();
        }

        await using (var db = new ApplicationDbContext(options))
        {
            var user = await db.Users.Include(u => u.Reactions)
                .SingleAsync(u => u.Id == author.Id);
            Assert.Empty(user.Reactions!);
        }
    }

    [Fact]
    public async Task Unique_filtered_indexes_exist_on_both_reaction_targets()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new ApplicationDbContext(Options(connection));
        await db.Database.EnsureCreatedAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM pragma_index_list('Reactions') WHERE \"unique\" = 1 AND partial = 1";
        await using var reader = await command.ExecuteReaderAsync();
        var names = new List<string>();
        while (await reader.ReadAsync())
            names.Add(reader.GetString(0));

        Assert.Contains("IX_Reactions_User_Post", names);
        Assert.Contains("IX_Reactions_User_Comment", names);
    }

    [Fact]
    public async Task Deleted_comment_flag_survives_sqlite_roundtrip()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = Options(connection);
        var author = Sample.User();
        var forum = Sample.Forum();
        var post = Sample.Post(author, forum);
        var comment = new Comment("Comment", post, author, Sample.Now);
        comment.MarkAsDeleted();

        await using (var db = new ApplicationDbContext(options))
        {
            await db.Database.EnsureCreatedAsync();
            db.Users.Add(author);
            db.Forums.Add(forum);
            db.Posts.Add(post);
            db.Comments.Add(comment);
            await db.SaveChangesAsync();
        }

        await using (var db = new ApplicationDbContext(options))
        {
            var saved = await db.Comments.SingleAsync(c => c.Id == comment.Id);
            Assert.True(saved.IsDeleted);
            Assert.Equal("*Este contenido ha sido eliminado.*", saved.Body.Value);
        }
    }
}
