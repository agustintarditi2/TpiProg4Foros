using MyApp.Domain.Entities;

namespace MyApp.Application.Models;

public record PostDTO(
    Guid id,
    Guid Poster,
    Guid Forum,
    DateTimeOffset Date,
    string Body
)
{
    public PostDTO Create(Post post)
    {
        return new PostDTO(
            post.Id,
            post.Poster,
            post.ForumId,
            post.DateEdited ?? post.dateCreated,
            post.Body
        )
    }
}