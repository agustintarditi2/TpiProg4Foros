using MyApp.Domain.Entities;
using MyApp.Domain.ValueObjects;

namespace MyApp.Application.Models;

public record PostDTO(
    Guid id,
    Guid Poster,
    Guid Forum,
    DateTimeOffset Date,
    bool Edited,
    bool Hidden,
    string Body
)
{
    public static PostDTO Create(Post post)
    {
        return new PostDTO(
            post.Id,
            post.Poster,
            post.ForumId,
            post.DateEdited ?? post.DateCreated,
            post.Edited,
            post.Hidden,
            post.Body.ToString()
        );
    }
}