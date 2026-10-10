
namespace MyApp.Domain.ValueObjects;

public record Reaction
{
    public PostId? Post {get; init;}
    public CommentId? Comment {get; init;}
    public UserId User {get; init;}
    public bool Type {get; init;}

    public Reaction(PostId post, UserId user, bool type)
    {
        Post = post;
        Comment = null;
        User = user;
        Type = type;
    }
    public Reaction(CommentId comment, UserId user, bool type)
    {
        Post = null;
        Comment = comment;
        User = user;
        Type = type;
    }
}