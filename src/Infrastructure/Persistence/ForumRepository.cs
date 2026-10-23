using Microsoft.EntityFrameworkCore;
using MyApp.Application.Interfaces;
using MyApp.Domain.Entities;
using MyApp.Domain.ValueObjects;

namespace MyApp.Infrastructure.Persistence.Repositories;

// implementa el acceso a datos de los foros con entity
public sealed class ForumRepository : IForumRepository
{
    // contexto utilizado para acceder a la base de datos
    private readonly ApplicationDbContext _context;

    // recibe el contexto mediante inyeccion de dependencias
    public ForumRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // obtiene todos los foros
    public async Task<List<Forum>> Get(CancellationToken ct)
    {
        return await _context.Forums.ToListAsync(ct);
    }

    // obtiene un foro por su id
    public async Task<Forum?> GetById(ForumId id, CancellationToken ct)
    {
        return await _context.Forums
            .FirstOrDefaultAsync(f => f.Id == id, ct);
    }

    // agrega un nuevo foro
    public async Task<Forum> Add(Forum entity, CancellationToken ct)
    {
        _context.Forums.Add(entity);

        await SaveChanges(ct);

        return entity;
    }

    // modifica un foro
    public async Task Update(Forum entity, CancellationToken ct)
    {
        _context.Forums.Update(entity);

        await SaveChanges(ct);
    }

    // elimina un foro
    public async Task Delete(Forum entity, CancellationToken ct)
    {
        _context.Forums.Remove(entity);

        await SaveChanges(ct);
    }

    // guarda los cambios en la base de datos
    private async Task<int> SaveChanges(CancellationToken ct)
    {
        return await _context.SaveChangesAsync(ct);
    }
}