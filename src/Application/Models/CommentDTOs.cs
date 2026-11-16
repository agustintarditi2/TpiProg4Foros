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
    bool IsDeleted,
    string Body,
    Guid? RepliedComment = null
)
{
    public static CommentDTO Create(Comment comment)
    {
         string body = comment.Invisible
        ? "*Este comentario está oculto.*"
        : comment.Body.ToString();

        return new CommentDTO(
            comment.Id,
            comment.Poster,
            comment.ParentPost,
            comment.DateEdited ?? comment.DateCreated,
            comment.Edited,
            comment.Invisible,
            comment.IsDeleted,
            body,
            comment.RepliedComment
        );
    }
}


public record CreateCommentDTO(
    Guid ParentPost,
    string Body,
    Guid? RepliedComment = null
);