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
    [Fact]
    public async Task Identity_migration_preserves_existing_reactions_and_generates_new_ids()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = Options(connection);

        // prepara el esquema anterior con identificadores repetidos entre usuarios
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = """
                CREATE TABLE "Users" ("Id" TEXT NOT NULL PRIMARY KEY);
                CREATE TABLE "Reactions" (
                    "UserId" TEXT NOT NULL,
                    "Id" INTEGER NOT NULL,
                    "Comment" TEXT NULL,
                    "Post" TEXT NULL,
                    "Type" INTEGER NOT NULL,
                    "User" TEXT NOT NULL,
                    CONSTRAINT "PK_Reactions" PRIMARY KEY ("UserId", "Id"),
                    FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
                );
                CREATE TABLE "__EFMigrationsHistory" (
                    "MigrationId" TEXT NOT NULL PRIMARY KEY,
                    "ProductVersion" TEXT NOT NULL
                );
                INSERT INTO "Users" ("Id")
                    VALUES ('11111111-1111-1111-1111-111111111111'),
                           ('22222222-2222-2222-2222-222222222222');
                INSERT INTO "Reactions" ("UserId", "Id", "Comment", "Post", "Type", "User")
                    VALUES
                    ('11111111-1111-1111-1111-111111111111', 1, NULL,
                     '33333333-3333-3333-3333-333333333333', 1,
                     '11111111-1111-1111-1111-111111111111'),
                    ('22222222-2222-2222-2222-222222222222', 1, NULL,
                     '33333333-3333-3333-3333-333333333333', 0,
                     '22222222-2222-2222-2222-222222222222');
                """;
            await command.ExecuteNonQueryAsync();
        }

        await using (var db = new ApplicationDbContext(options))
        {
            // marca las migraciones anteriores como aplicadas
            foreach (var id in db.Database.GetMigrations()
                .Where(m => m != "20261116021729_FixReactionIdentity"))
            {
                await using var cmd = connection.CreateCommand();
                cmd.CommandText = "INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES (@id, '10.0.12')";
                cmd.Parameters.AddWithValue("@id", id);
                await cmd.ExecuteNonQueryAsync();
            }

            // aplica la migracion real sobre una tabla con registros
            await db.Database.MigrateAsync();
        }

        await using (var command = connection.CreateCommand())
        {
            command.CommandText = "SELECT COUNT(*) FROM Reactions";
            Assert.Equal(2L, (long)(await command.ExecuteScalarAsync())!);

            command.CommandText = "SELECT COUNT(DISTINCT Id) FROM Reactions";
            Assert.Equal(2L, (long)(await command.ExecuteScalarAsync())!);

            // la nueva clave debe generarse sin aportar el campo id
            command.CommandText = """
                INSERT INTO Reactions (UserId, Comment, Post, Type, User)
                VALUES ('11111111-1111-1111-1111-111111111111', NULL,
                        '44444444-4444-4444-4444-444444444444', 1,
                        '11111111-1111-1111-1111-111111111111')
                """;
            await command.ExecuteNonQueryAsync();

            command.CommandText = "SELECT COUNT(*) FROM Reactions";
            Assert.Equal(3L, (long)(await command.ExecuteScalarAsync())!);
        }
    }

}
