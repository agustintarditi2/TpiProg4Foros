using MyApp.Domain.Entities;
using MyApp.Domain.ValueObjects;

namespace MyApp.Application.Interfaces;

public interface ICommentRepository
{
    Task<List<Comment>> GetByPost(PostId id, CancellationToken ct);
    Task<List<Comment>> GetByThread(CommentId id, CancellationToken ct);
    Task<List<Comment>> GetByPoster(UserId id, CancellationToken ct);
    Task<Comment?> GetById(CommentId id, CancellationToken ct);
    Task<Comment> Add(Comment entity, CancellationToken ct);
    Task Delete(Comment entity, CancellationToken ct);
    Task Update(Comment entity, CancellationToken ct);
}