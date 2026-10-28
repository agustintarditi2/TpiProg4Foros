using MyApp.Domain.ValueObjects;
using MyApp.Domain.Exceptions;

namespace MyApp.Domain.Entities;

public sealed class Post : TextEntity
{
    public PostId Id {get; private set;}
    public UserId Poster {get; private set;}
    public ForumId ForumId {get; private set;}

    private Post() {Body = null!;}
    public Post(string body, UserId poster, ForumId forum, DateTimeOffset dateCreated)
    {
        if (forum.IsEmpty)
            throw new DomainException("A post must belong to a forum.");

        Body = BodyText.Create(body);
        Id = PostId.New();
        Poster = poster;
        ForumId = forum;
        DateCreated = dateCreated;
        DateEdited = null;
        Edited = false;
        Hidden = false;
    }

    public void EditBody(string newBody, DateTimeOffset dateEdited)
    {
        Body = BodyText.Create(newBody);
        DateEdited = dateEdited;
        Edited = true;
    }

    public void HideOrUnhide(bool hidden)
    {
        Hidden = hidden;
    }

}