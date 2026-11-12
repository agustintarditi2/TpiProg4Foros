using Microsoft.EntityFrameworkCore;
using MyApp.Application.Interfaces;
using MyApp.Domain.Entities;
using MyApp.Domain.ValueObjects;
using MyApp.Domain.Enums;

namespace MyApp.Infrastructure.Persistence.Repositories;

public sealed class BanRepository : IBanRepository
{
    private readonly ApplicationDbContext _context;

    public BanRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Ban>> Get(CancellationToken ct)
{
    var bans = await _context.Bans.ToListAsync(ct);
    return bans;
}

public async Task<Ban?> GetById(BanId id, CancellationToken ct)
{
    var ban = await _context.Bans.FirstOrDefaultAsync(b => b.Id == id, ct);
    return ban;
}

public async Task<Ban> Add(Ban entity, CancellationToken ct)
{
    _context.Bans.Add(entity);
    await SaveChanges(ct);
    return entity;
}

public async Task Delete(Ban entity, CancellationToken ct)
{
    _context.Bans.Remove(entity);
    await SaveChanges(ct);
}

public async Task Update(Ban entity, CancellationToken ct)
{
    _context.Bans.Update(entity);
    await SaveChanges(ct);
}

internal Task<int> SaveChanges(CancellationToken cancellationToken)
{
    return _context.SaveChangesAsync(cancellationToken);
}

public async Task<List<Ban>> GetActiveByUserId(UserId userId, CancellationToken ct)
{
    return await _context.Bans
        .Where(b => b.UserId == userId && b.Status == BanStatus.Activo)
        .ToListAsync(ct);
}
}