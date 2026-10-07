using MyApp.Domain.Entities;
using MyApp.Domain.ValueObjects;

namespace MyApp.Application.Interfaces;

public interface IPostRepository
{
    Task<List<Post>> GetByForum(ForumId id, CancellationToken ct);
    Task<List<Post>> GetByPoster(UserId id, CancellationToken ct);
    Task<Post?> GetById(PostId id, CancellationToken ct);
    Task<Post> Add(Post entity, CancellationToken ct);
    Task Delete(Post entity, CancellationToken ct);
    Task Update(Post entity, CancellationToken ct);
}