using MyApp.Application.Interfaces;
using MyApp.Domain.Entities;
using MyApp.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

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
        return await _context.Posts.Where(p => p.ForumId == id).ToListAsync(ct);
    }



    public async Task<List<Post>> GetByPoster(UserId id, CancellationToken ct)
    {
        return await _context.Posts.Where(p => p.Poster == id).ToListAsync(ct);
    }



    public async Task<Post?> GetById(PostId id, CancellationToken ct)
    {
        return await _context.Posts.FirstOrDefaultAsync(p => p.Id == id, ct);
    }



    public async Task<Post> Add(Post entity, CancellationToken ct)
    {
        _context.Posts.Add(entity);
        await SaveChanges(ct);
        return entity;
    }



    public async Task Delete(Post entity, CancellationToken ct)
    {
        _context.Posts.Remove(entity);
        await SaveChanges(ct);
        return;
    }



    public async Task Update(Post entity, CancellationToken ct)
    {
        _context.Posts.Update(entity);
        await SaveChanges(ct);
        return;
    }


    
    private async Task<int> SaveChanges(CancellationToken cancellationToken)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}