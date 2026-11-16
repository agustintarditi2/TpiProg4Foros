using MyApp.Domain.Exceptions;
using MyApp.Domain.ValueObjects;

namespace MyApp.Domain.Entities;

public sealed class Comment : TextEntity
{
    public CommentId Id {get; private set;}

    public PostId ParentPost {get; private set;}
    public CommentId? RepliedComment {get; private set;}
    public bool IsDeleted { get; private set; } = false;

    private Comment() {Body = null!;}
    public Comment(
        string body, 
        Post post, 
        User user, 
        DateTimeOffset dateCreated, 
        Comment? repliedComment = null)
    {
        if (repliedComment is not null && repliedComment.ParentPost != post.Id)
            throw new DomainException("A reply must belong to the same post as its parent.");
        if (repliedComment is not null)
            RepliedComment = repliedComment.Id;
        Body = BodyText.Create(body);
        Id = CommentId.New();
        ParentPost = post.Id;
        Poster = user.Id;
        DateCreated = dateCreated;
        DateEdited = null;
    }
    public void MarkAsDeleted()
    {
        if (IsDeleted)
            throw new DomainException(
                "Comment is already deleted.");

        base.Delete();
        IsDeleted = true;
}
}