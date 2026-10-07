using MyApp.Domain.ValueObjects;

namespace MyApp.Domain.Entities;

public sealed class Post
{
    public PostId Id {get; private set;}
    public DateTimeOffset DateCreated {get; private set;}
    public DateTimeOffset? DateEdited {get; private set;}
    public UserId Poster {get; private set;}
    public BodyText Body {get; private set;}
    public ForumId ForumId {get; private set;}
    public bool Edited {get; private set;}

    private Post() {Body = null!;}
    public Post(string body, User poster, ForumId forum, DateTimeOffset dateCreated)
    {   Body = BodyText.Create(body);
        Id = PostId.New();
        Poster = poster.Id;
        ForumId = forum;
        DateCreated = dateCreated;
        DateEdited = null;
        Edited = false;
    }
    public void EditBody(string newBody, DateTimeOffset dateEdited)
    {
        Body = BodyText.Create(newBody);
        DateEdited = dateEdited;
        Edited = true;
    }


}