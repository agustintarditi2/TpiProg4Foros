using MyApp.Application.Interfaces;
using MyApp.Domain.Entities;
using MyApp.Domain.ValueObjects;

namespace MyApp.Infrastructure.Persistence.Repositories;

public sealed class PostRepository : IPostRepository
{
    private readonly ApplicationDbContext _context;
    public PostRepository(ApplicationDbContext context)
    {
        _context = context;
    }


    public async Task<List<Post>> GetByForum(ForumId id, CancellationToken ct)
    {
        
    }



    public async Task<List<Post>> GetByPoster(UserId id, CancellationToken ct)
    {
        
    }

    

    public async Task<Post?> GetById(PostId id, CancellationToken ct)
    {
        
    }



    public async Task<Post> Add(Post entity, CancellationToken ct)
    {
        
    }



    public async Task Delete(Post entity, CancellationToken ct)
    {
        
    }



    public async Task Update(Post entity, CancellationToken ct)
    {
        
    }


    
    private async Task<int> SaveChanges(CancellationToken cancellationToken)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}