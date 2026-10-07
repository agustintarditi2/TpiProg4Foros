using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using MyApp.Application.Interfaces;
using MyApp.Domain.Entities;
using MyApp.Domain.ValueObjects;

namespace MyApp.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<List<User>> Get(CancellationToken ct)
    {
        var users = await _context.Users.ToListAsync(ct);
        return users;
    }
    public async Task<User?> GetById(UserId id, CancellationToken ct)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        return user;
    }
    public async Task<User?> GetByEmail(EmailAddress email, CancellationToken ct)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
        return user;
    }
    public async Task<User> Add(User entity, CancellationToken ct)
    {
        _context.Users.Add(entity);
        await SaveChanges(ct);
        return entity;
    }
    public async void Delete(User entity, CancellationToken ct)
    {
        _context.Users.Remove(entity);
        await SaveChanges(ct);
    }
    public async void Update(User entity, CancellationToken ct)
    {
        _context.Users.Update(entity);
        await SaveChanges(ct);
    }
    public async Task<bool> EmailExists(EmailAddress email)
    {
        var exists = await _context.Users.FirstOrDefaultAsync(u => u.Email == email) ?? null;
        if (exists is null)
            return false;
        else return true;
    }
    private async Task<int> SaveChanges(CancellationToken cancellationToken)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}