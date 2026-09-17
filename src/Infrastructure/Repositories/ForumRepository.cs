using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

// ForumRepository implementa la interfaz IForumRepository, por lo que hace todo lo que esta definido en la interfaz
public class ForumRepository : IForumRepository
{
    // variable para guardar el applicationContext
    // privada para encapsular el detalle interno de como el repositorio trabaja con la base de datos
    // readonly para que el contexto que se asigna, despues no pueda ser reemplazado por otro contexto
    private readonly ApplicationContext _context;

    public ForumRepository(ApplicationContext context)
    {
        _context = context;
    }

    public async Task<List<Forum>> GetAllAsync()
    {
        return await _context.Forums.ToListAsync();
    }

    public async Task<Forum?> GetByIdAsync(int id)
    {
        return await _context.Forums.FindAsync(id);
    }

    public async Task AddAsync(Forum forum)
    {
        await _context.Forums.AddAsync(forum);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Forum forum)
    {
        _context.Forums.Update(forum);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Forum forum)
    {
        _context.Forums.Remove(forum);
        await _context.SaveChangesAsync();
    }
}