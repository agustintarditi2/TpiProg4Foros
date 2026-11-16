using MyApp.Application.Models;
using MyApp.Domain.Entities;
using MyApp.Domain.Exceptions;
using TpiProg4Foros.Tests.Support;
using Xunit;

namespace TpiProg4Foros.Tests.Domain;

public class CommentSoftDeleteTests
{
    [Fact]
    public void Deletion_keeps_comment_and_marks_it_as_deleted()
    {
        var author = Sample.User();
        var post = Sample.Post(author, Sample.Forum());
        var comment = new Comment("Original", post, author, Sample.Now);

        comment.MarkAsDeleted();

        Assert.True(comment.IsDeleted);
        Assert.Equal("*Este contenido ha sido eliminado.*", comment.Body.Value);
        Assert.Equal(post.Id, comment.ParentPost);
        Assert.Equal(author.Id, comment.Poster);
    }

    [Fact]
    public void Deleting_comment_twice_throws()
    {
        var author = Sample.User();
        var comment = new Comment("Original", Sample.Post(author, Sample.Forum()), author, Sample.Now);
        comment.MarkAsDeleted();

        Assert.Throws<DomainException>(() => comment.MarkAsDeleted());
    }

    [Fact]
    public void Redacted_dto_never_changes_saved_comment_body()
    {
        var author = Sample.User();
        var comment = new Comment("Secret original", Sample.Post(author, Sample.Forum()), author, Sample.Now);
        comment.ToggleHidden(true);

        var dto = CommentDTO.Create(comment);

        Assert.True(dto.Invisible);
        Assert.Equal("*Este comentario está oculto.*", dto.Body);
        Assert.Equal("Secret original", comment.Body.Value);
        comment.ToggleHidden(false);
        Assert.Equal("Secret original", CommentDTO.Create(comment).Body);
    }

    [Fact]
    public void Comment_dto_exposes_deleted_flag_without_losing_replies()
    {
        var author = Sample.User();
        var post = Sample.Post(author, Sample.Forum());
        var parent = new Comment("Original", post, author, Sample.Now);
        var reply = new Comment("Reply", post, author, Sample.Now, parent);
        parent.MarkAsDeleted();

        Assert.True(CommentDTO.Create(parent).IsDeleted);
        Assert.False(CommentDTO.Create(reply).IsDeleted);
        Assert.Equal(parent.Id, reply.RepliedComment);
    }
}
