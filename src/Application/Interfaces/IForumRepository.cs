using Domain.Entities;

namespace Application.Interfaces;

public interface IForumRepository
{
    Task<List<Forum>> GetAllAsync();

    Task<Forum?> GetByIdAsync(int id);

    Task AddAsync(Forum forum);

    Task UpdateAsync(Forum forum);

    Task DeleteAsync(Forum forum);
}