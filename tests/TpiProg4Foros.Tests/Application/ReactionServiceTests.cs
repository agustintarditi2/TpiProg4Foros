using MyApp.Application.Interfaces;
using MyApp.Application.Services;
using MyApp.Domain.Entities;
using MyApp.Domain.Enums;
using MyApp.Domain.Exceptions;
using TpiProg4Foros.Tests.Support;
using Xunit;

namespace TpiProg4Foros.Tests.Application;

public class ReactionServiceTests
{
    private static readonly CancellationToken Ct = CancellationToken.None;
    private readonly FakeUsers _users = new();
    private readonly FakePosts _posts = new();
    private readonly FakeComments _comments = new();
    private readonly FakeForums _forums = new();
    private readonly IBanRepository _banRepo;
    private readonly BanRepositoryProxy _banSpy;

    public ReactionServiceTests()
    {
        (_banRepo, _banSpy) = BanTestSupport.CreateRepository();
    }

    private ReactionServices Service => new(
        _users, _posts, _comments, _forums,
        new BanServices(_banRepo, _users));

    private (User User, Forum Forum, Post Post, Comment Comment) Seed()
    {
        var user = Sample.User();
        var forum = Sample.Forum();
        var post = Sample.Post(user, forum);
        var comment = new Comment("A comment", post, user, Sample.Now);
        _users.Items[user.Id.Value] = user;
        _forums.Items[forum.Id.Value] = forum;
        _posts.Items[post.Id.Value] = post;
        _comments.Items[comment.Id.Value] = comment;
        return (user, forum, post, comment);
    }

    [Fact]
    public async Task Set_post_like_and_dislike_without_duplicates()
    {
        var (user, _, post, _) = Seed();
        await Service.SetPostReaction(post.Id.Value, user.Id.Value, true, Ct);
        await Service.SetPostReaction(post.Id.Value, user.Id.Value, true, Ct);
        await Service.SetPostReaction(post.Id.Value, user.Id.Value, false, Ct);

        Assert.Single(user.Reactions!);
        Assert.False(user.Reactions![0].Type);
        Assert.Equal(3, _users.Updates);
    }

    [Fact]
    public async Task Set_comment_like_and_dislike_without_duplicates()
    {
        var (user, _, _, comment) = Seed();
        await Service.SetCommentReaction(comment.Id.Value, user.Id.Value, true, Ct);
        await Service.SetCommentReaction(comment.Id.Value, user.Id.Value, false, Ct);

        Assert.Single(user.Reactions!);
        Assert.False(user.Reactions![0].Type);
    }

    [Fact]
    public async Task Remove_reactions_is_idempotent_even_when_hidden_or_deleted()
    {
        var (user, _, post, comment) = Seed();
        await Service.SetPostReaction(post.Id.Value, user.Id.Value, true, Ct);
        await Service.SetCommentReaction(comment.Id.Value, user.Id.Value, true, Ct);
        post.ToggleHidden(true);
        comment.MarkAsDeleted();

        await Service.RemovePostReaction(post.Id.Value, user.Id.Value, Ct);
        await Service.RemovePostReaction(post.Id.Value, user.Id.Value, Ct);
        await Service.RemoveCommentReaction(comment.Id.Value, user.Id.Value, Ct);
        await Service.RemoveCommentReaction(comment.Id.Value, user.Id.Value, Ct);

        Assert.Empty(user.Reactions!);
    }

    [Fact]
    public async Task Set_reaction_requires_existing_user_and_post()
    {
        var (user, _, post, _) = Seed();
        await Assert.ThrowsAsync<NotFoundException>(() =>
            Service.SetPostReaction(post.Id.Value, Guid.NewGuid(), true, Ct));
        await Assert.ThrowsAsync<NotFoundException>(() =>
            Service.SetPostReaction(Guid.NewGuid(), user.Id.Value, true, Ct));
    }

    [Fact]
    public async Task Set_post_reaction_rejects_hidden_post_and_inactive_forum()
    {
        var (user, forum, post, _) = Seed();
        post.ToggleSuppressed(true);
        await Assert.ThrowsAsync<DomainException>(() =>
            Service.SetPostReaction(post.Id.Value, user.Id.Value, true, Ct));
        post.ToggleSuppressed(false);
        forum.Deactivate();
        await Assert.ThrowsAsync<DomainException>(() =>
            Service.SetPostReaction(post.Id.Value, user.Id.Value, true, Ct));

        Assert.Empty(user.Reactions!);
        Assert.Equal(0, _users.Updates);
    }

    [Fact]
    public async Task Set_comment_reaction_rejects_deleted_and_hidden_comments()
    {
        var (user, _, _, comment) = Seed();
        comment.ToggleHidden(true);
        await Assert.ThrowsAsync<DomainException>(() =>
            Service.SetCommentReaction(comment.Id.Value, user.Id.Value, true, Ct));
        comment.ToggleHidden(false);
        comment.MarkAsDeleted();
        await Assert.ThrowsAsync<DomainException>(() =>
            Service.SetCommentReaction(comment.Id.Value, user.Id.Value, true, Ct));

        Assert.Empty(user.Reactions!);
    }

    [Fact]
    public async Task Set_comment_reaction_rejects_hidden_parent_post()
    {
        var (user, _, post, comment) = Seed();
        post.ToggleHidden(true);
        await Assert.ThrowsAsync<DomainException>(() =>
            Service.SetCommentReaction(comment.Id.Value, user.Id.Value, true, Ct));
    }

    [Fact]
    public async Task Inactive_user_cannot_react()
    {
        var (user, _, post, _) = Seed();
        user.DeactivateAccount("disabled-password-hash");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            Service.SetPostReaction(post.Id.Value, user.Id.Value, true, Ct));
        Assert.Empty(user.Reactions!);
    }

    [Fact]
    public async Task Banned_user_cannot_react()
    {
        var (user, _, post, _) = Seed();
        var admin = Sample.User("admin@example.com");
        admin.SetRole(UserRole.Admin);
        _banSpy.Items[Guid.NewGuid()] = new Ban(user, admin, 5, "Violation");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            Service.SetPostReaction(post.Id.Value, user.Id.Value, true, Ct));
        Assert.Empty(user.Reactions!);
    }

    [Fact]
    public async Task Reaction_is_always_associated_with_authenticated_user()
    {
        var (user, _, post, _) = Seed();
        var other = Sample.User("other@example.com");
        _users.Items[other.Id.Value] = other;

        await Service.SetPostReaction(post.Id.Value, user.Id.Value, true, Ct);
        await Service.SetPostReaction(post.Id.Value, other.Id.Value, false, Ct);

        Assert.Single(user.Reactions!);
        Assert.Single(other.Reactions!);
        Assert.Equal(user.Id, user.Reactions![0].User);
        Assert.Equal(other.Id, other.Reactions![0].User);
    }
}
