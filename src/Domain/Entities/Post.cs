using MyApp.Domain.ValueObjects;

namespace MyApp.Domain.Entities;

public sealed class Post : TextEntity
{
    public PostId Id {get; private set;}
    public UserId Poster {get; private set;}
    public ForumId ForumId {get; private set;}

    private Post() {Body = null!;}
    public Post(string body, UserId poster, ForumId forum, DateTimeOffset dateCreated)
    {   Body = BodyText.Create(body);
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

    public void HideOrUnhide()
    {
        Hidden = !Hidden;
    }

}