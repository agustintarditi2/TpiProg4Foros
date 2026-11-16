using MyApp.Application.Models;
using MyApp.Application.Services;
using MyApp.Domain.Entities;
using MyApp.Domain.Exceptions;
using TpiProg4Foros.Tests.Support;
using Xunit;

namespace TpiProg4Foros.Tests.Application;

public class CommentServiceTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;
    private readonly FakeUsers _users = new();
    private readonly FakePosts _posts = new();
    private readonly FakeComments _comments = new();
    private readonly FakeForums _forums = new();
    private readonly FakeClock _clock = new();

    private CommentServices Service => new(
        _comments, _posts, _users, _forums, _clock, new FakeSanitizer());

    private (User Author, Forum Forum, Post Post) Seed()
    {
        var user = Sample.User();
        var forum = Sample.Forum();
        var post = Sample.Post(user, forum);
        _users.Items[user.Id.Value] = user;
        _forums.Items[forum.Id.Value] = forum;
        _posts.Items[post.Id.Value] = post;
        return (user, forum, post);
    }

    [Fact]
    public async Task Create_saves_comment_in_active_forum()
    {
        var (author, _, post) = Seed();
        var result = await Service.Create(new CreateCommentDTO(post.Id.Value, "  New comment  "), author.Id.Value, Ct);

        Assert.Single(_comments.Items);
        Assert.Equal("New comment", result.Body);
        Assert.False(result.IsDeleted);
    }

    [Fact]
    public async Task Create_rejects_hidden_post()
    {
        var (author, _, post) = Seed();
        post.ToggleHidden(true);

        await Assert.ThrowsAsync<DomainException>(() =>
            Service.Create(new CreateCommentDTO(post.Id.Value, "Text"), author.Id.Value, Ct));
        Assert.Empty(_comments.Items);
    }

    [Fact]
    public async Task Create_rejects_inactive_forum()
    {
        var (author, forum, post) = Seed();
        forum.Deactivate();

        await Assert.ThrowsAsync<DomainException>(() =>
            Service.Create(new CreateCommentDTO(post.Id.Value, "Text"), author.Id.Value, Ct));
        Assert.Empty(_comments.Items);
    }

    [Fact]
    public async Task Create_rejects_hidden_reply_target()
    {
        var (author, _, post) = Seed();
        var parent = new Comment("Parent", post, author, Sample.Now);
        parent.ToggleSuppressed(true);
        _comments.Items[parent.Id.Value] = parent;

        await Assert.ThrowsAsync<DomainException>(() =>
            Service.Create(new CreateCommentDTO(post.Id.Value, "Reply", parent.Id.Value), author.Id.Value, Ct));
        Assert.Single(_comments.Items);
    }

    [Fact]
    public async Task Reply_to_deleted_comment_is_allowed()
    {
        var (author, _, post) = Seed();
        var parent = new Comment("Parent", post, author, Sample.Now);
        parent.MarkAsDeleted();
        _comments.Items[parent.Id.Value] = parent;

        var reply = await Service.Create(
            new CreateCommentDTO(post.Id.Value, "Reply", parent.Id.Value), author.Id.Value, Ct);
        Assert.Equal(parent.Id.Value, reply.RepliedComment);
        Assert.Equal(2, _comments.Items.Count);
    }

    [Fact]
    public async Task Only_author_can_edit_or_delete_comment()
    {
        var (author, _, post) = Seed();
        var comment = new Comment("Original", post, author, Sample.Now);
        _comments.Items[comment.Id.Value] = comment;

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            Service.Update(comment.Id.Value, "Changed", Guid.NewGuid(), Ct));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            Service.Delete(comment.Id.Value, Guid.NewGuid(), Ct));
        Assert.Equal("Original", comment.Body.Value);

        await Service.Update(comment.Id.Value, "Changed", author.Id.Value, Ct);
        Assert.Equal("Changed", comment.Body.Value);
        await Service.Delete(comment.Id.Value, author.Id.Value, Ct);
        Assert.True(comment.IsDeleted);
        Assert.Equal(2, _comments.Updates);
    }

    [Fact]
    public async Task Cannot_edit_or_delete_twice_after_soft_deletion()
    {
        var (author, _, post) = Seed();
        var comment = new Comment("Original", post, author, Sample.Now);
        _comments.Items[comment.Id.Value] = comment;

        await Service.Delete(comment.Id.Value, author.Id.Value, Ct);
        Assert.Single(_comments.Items);
        Assert.Equal("*Este contenido ha sido eliminado.*", (await Service.GetById(comment.Id.Value, Ct)).Body);

        await Assert.ThrowsAsync<DomainException>(() =>
            Service.Update(comment.Id.Value, "Changed", author.Id.Value, Ct));
        await Assert.ThrowsAsync<DomainException>(() =>
            Service.Delete(comment.Id.Value, author.Id.Value, Ct));
        Assert.Equal(1, _comments.Updates);
    }

    [Fact]
    public async Task Cannot_edit_suppressed_comment_or_comment_on_hidden_post()
    {
        var (author, _, post) = Seed();
        var comment = new Comment("Original", post, author, Sample.Now);
        _comments.Items[comment.Id.Value] = comment;

        comment.ToggleSuppressed(true);
        await Assert.ThrowsAsync<DomainException>(() =>
            Service.Update(comment.Id.Value, "Changed", author.Id.Value, Ct));
        comment.ToggleSuppressed(false);
        post.ToggleHidden(true);
        await Assert.ThrowsAsync<DomainException>(() =>
            Service.Update(comment.Id.Value, "Changed", author.Id.Value, Ct));

        Assert.Equal(0, _comments.Updates);
        Assert.Equal("Original", comment.Body.Value);
    }
}
