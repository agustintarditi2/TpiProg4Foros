using MyApp.Domain.Entities;
using MyApp.Domain.ValueObjects;

namespace MyApp.Application.Models;

public record CommentDTO(
    Guid id,
    Guid Poster,
    Guid ParentPost,
    DateTimeOffset Date,
    bool Edited,
    bool Invisible,
    string Body,
    Guid? RepliedComment = null
)
{
    public static CommentDTO Create(Comment comment)
    {
        if (comment.Invisible) comment.Redact();
        return new CommentDTO(
            comment.Id,
            comment.Poster,
            comment.ParentPost,
            comment.DateEdited ?? comment.DateCreated,
            comment.Edited,
            comment.Invisible,
            comment.Body.ToString(),
            comment.RepliedComment
        );
    }
}


public record CreateCommentDTO(
    Guid ParentPost,
    string Body,
    Guid? RepliedComment = null
);