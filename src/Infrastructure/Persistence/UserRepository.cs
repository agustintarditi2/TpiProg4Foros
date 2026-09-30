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
    public async Task<List<User>> Get()
    {
        var users = _context.Users.ToList();
        var ct = new CancellationToken(); //DEBUG!! Quitar en implementación!!
        await SaveChanges(ct);
        return users;
    }
    public async Task<User?> GetById(UserId id)
    {
        var user = _context.Users.FirstOrDefault(u => u.Id == id);
        var ct = new CancellationToken(); //DEBUG!! Quitar en implementación!!
        await SaveChanges(ct);
        return user;
    }
    public async Task<User> Add(User entity)
    {
        _context.Users.Add(entity);
        var ct = new CancellationToken(); //DEBUG!! Quitar en implementación!!
        await SaveChanges(ct);
        return entity;
    }
    public async void Delete(User entity)
    {
        _context.Users.Remove(entity);
        var ct = new CancellationToken(); //DEBUG!! Quitar en implementación!!
        await SaveChanges(ct);
    }
    public async void Update(User entity)
    {
        _context.Users.Update(entity);
        var ct = new CancellationToken(); //DEBUG!! Quitar en implementación!!
        await SaveChanges(ct);
    }
    public async Task<bool> EmailExists(EmailAddress email)
    {
        var exists = await _context.Users.FirstOrDefaultAsync(u => u.Email == email) ?? null;
        if (exists is null)
            return false;
        else return true;
    }
    internal Task<int> SaveChanges(CancellationToken cancellationToken)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}