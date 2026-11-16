using MyApp.Domain.Exceptions;
using MyApp.Domain.ValueObjects;
using TpiProg4Foros.Tests.Support;
using Xunit;

namespace TpiProg4Foros.Tests.Domain;

public class ReactionDomainTests
{
    [Fact]
    public void Post_like_is_idempotent_and_dislike_updates_same_reaction()
    {
        var user = Sample.User();
        var post = Sample.Post(user, Sample.Forum());

        user.SetPostReaction(post.Id, true);
        user.SetPostReaction(post.Id, true);
        Assert.Single(user.Reactions!);
        Assert.True(user.Reactions![0].Type);

        user.SetPostReaction(post.Id, false);
        Assert.Single(user.Reactions!);
        Assert.False(user.Reactions![0].Type);
        Assert.Equal(post.Id, user.Reactions![0].Post);
        Assert.Null(user.Reactions![0].Comment);
        Assert.Equal(user.Id, user.Reactions![0].User);
    }

    [Fact]
    public void Comment_like_is_idempotent_and_dislike_updates_same_reaction()
    {
        var user = Sample.User();
        var post = Sample.Post(user, Sample.Forum());
        var comment = new MyApp.Domain.Entities.Comment("Hello", post, user, Sample.Now);

        user.SetCommentReaction(comment.Id, false);
        user.SetCommentReaction(comment.Id, false);
        user.SetCommentReaction(comment.Id, true);

        Assert.Single(user.Reactions!);
        Assert.True(user.Reactions![0].Type);
        Assert.Equal(comment.Id, user.Reactions![0].Comment);
        Assert.Null(user.Reactions![0].Post);
    }

    [Fact]
    public void User_can_react_to_different_posts_and_comments()
    {
        var user = Sample.User();
        var forum = Sample.Forum();
        var p1 = Sample.Post(user, forum);
        var p2 = Sample.Post(user, forum);
        var comment = new MyApp.Domain.Entities.Comment("Hello", p1, user, Sample.Now);

        user.SetPostReaction(p1.Id, true);
        user.SetPostReaction(p2.Id, false);
        user.SetCommentReaction(comment.Id, true);

        Assert.Equal(3, user.Reactions!.Count);
    }

    [Fact]
    public void Removing_reactions_is_idempotent_and_target_specific()
    {
        var user = Sample.User();
        var post = Sample.Post(user, Sample.Forum());
        var comment = new MyApp.Domain.Entities.Comment("Hello", post, user, Sample.Now);

        user.SetPostReaction(post.Id, true);
        user.SetCommentReaction(comment.Id, false);
        user.RemovePostReaction(post.Id);
        user.RemovePostReaction(post.Id);
        Assert.Single(user.Reactions!);
        Assert.Equal(comment.Id, user.Reactions![0].Comment);

        user.RemoveCommentReaction(comment.Id);
        user.RemoveCommentReaction(comment.Id);
        Assert.Empty(user.Reactions!);
    }

    [Fact]
    public void Empty_target_identifiers_are_rejected()
    {
        var user = Sample.User();
        Assert.Throws<DomainException>(() => user.SetPostReaction((PostId)Guid.Empty, true));
        Assert.Throws<DomainException>(() => user.SetCommentReaction((CommentId)Guid.Empty, true));
        Assert.Throws<DomainException>(() => user.RemovePostReaction((PostId)Guid.Empty));
        Assert.Throws<DomainException>(() => user.RemoveCommentReaction((CommentId)Guid.Empty));
    }
}
