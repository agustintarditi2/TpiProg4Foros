using MyApp.Domain.Entities;
using MyApp.Domain.ValueObjects;

namespace MyApp.Application.Interfaces;

public interface IUserRepository
{
    Task<List<User>> Get();
    Task<User?> GetById(UserId id);
    Task<User> Add(User entity);
    void Delete(User entity);
    void Update(User entity);
    Task<bool> EmailExists(EmailAddress email);
    Task<int> SaveChanges();
}