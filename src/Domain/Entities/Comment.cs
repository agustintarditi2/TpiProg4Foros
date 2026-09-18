using MyApp.Domain.Exceptions;
using MyApp.Domain.ValueObjects;

namespace MyApp.Domain.Entities;

public sealed class Comment
{
    public CommentId Id {get; private set;}

    public PostId ParentPost {get; private set;}
    public CommentId? RepliedComment {get; private set;}
    public DateTimeOffset DateCreated {get; private set;}
    public UserId Poster {get; private set;}

    public DateTimeOffset DateEdited {get; private set;}
    public BodyText Body {get; private set;}

    private Comment() {Body = null!;}
    public Comment(string body, Post post, User user, DateTimeOffset dateCreated, Comment? comment)
    {
        if (comment is not null && comment.ParentPost != post.Id)
            throw new DomainException("A reply must belong to the same post as its parent.");
        if (comment != null)
            RepliedComment = comment.Id;
        Body = BodyText.Create(body);
        Id = CommentId.New();
        ParentPost = post.Id;
        Poster = user.Id;
        DateCreated = dateCreated;
        DateEdited = DateCreated;
    }
    public void EditBody(string newBody, DateTimeOffset dateEdited)
    {
        Body = BodyText.Create(newBody);
        DateEdited = dateEdited;
    }


}