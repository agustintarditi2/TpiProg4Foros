 using MyApp.Application.Interfaces;
using MyApp.Domain.Entities;
using MyApp.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace MyApp.Infrastructure.Persistence.Repositories;

public sealed class CommentRepository : ICommentRepository
{
    private readonly ApplicationDbContext _context;
    public CommentRepository(ApplicationDbContext context)
    {
        _context = context;
    }


    public async Task<List<Comment>> GetByPost(PostId id, CancellationToken ct)
    {
        return await _context.Comments.Where(p => p.ParentPost == id).ToListAsync(ct);
    }


    public async Task<List<Comment>> GetByThread(CommentId id, CancellationToken ct)
    {
        return await _context.Comments.Where(p => p.RepliedComment == id).ToListAsync(ct);
    }




    public async Task<List<Comment>> GetByPoster(UserId id, CancellationToken ct)
    {
        return await _context.Comments.Where(p => p.Poster == id).ToListAsync(ct);
    }



    public async Task<Comment?> GetById(CommentId id, CancellationToken ct)
    {
        return await _context.Comments.FirstOrDefaultAsync(p => p.Id == id, ct);
    }



    public async Task<Comment> Add(Comment entity, CancellationToken ct)
    {
        _context.Comments.Add(entity);
        await SaveChanges(ct);
        return entity;
    }



    public async Task Delete(Comment entity, CancellationToken ct)
    {
        _context.Comments.Remove(entity);
        await SaveChanges(ct);
        return;
    }



    public async Task Update(Comment entity, CancellationToken ct)
    {
        _context.Comments.Update(entity);
        await SaveChanges(ct);
        return;
    }


    
    private async Task<int> SaveChanges(CancellationToken cancellationToken)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}