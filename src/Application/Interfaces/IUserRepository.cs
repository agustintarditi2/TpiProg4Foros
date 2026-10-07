using MyApp.Domain.Entities;
using MyApp.Domain.ValueObjects;

namespace MyApp.Application.Interfaces;

public interface IUserRepository
{
    Task<List<User>> Get(CancellationToken ct);
    Task<User?> GetById(UserId id, CancellationToken ct);
    Task<User> Add(User entity, CancellationToken ct);
    Task Delete(User entity, CancellationToken ct);
    Task Update(User entity, CancellationToken ct);
    Task<bool> EmailExists(EmailAddress email);
}