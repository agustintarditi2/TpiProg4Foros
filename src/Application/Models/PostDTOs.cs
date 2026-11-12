using MyApp.Domain.Entities;
using MyApp.Domain.ValueObjects;

namespace MyApp.Application.Models;

public record PostDTO(
    Guid id,
    Guid Poster,
    Guid Forum,
    DateTimeOffset Date,
    bool Edited,
    bool Invisible,
    string Body
)
{
    public static PostDTO Create(Post post)
    {
        if (post.Invisible) post.Redact();
        return new PostDTO(
            post.Id,
            post.Poster,
            post.ForumId,
            post.DateEdited ?? post.DateCreated,
            post.Edited,
            post.Invisible,
            post.Body.ToString()
        );
    }
}


public record CreatePostDTO(
    Guid Forum,
    string Body
);