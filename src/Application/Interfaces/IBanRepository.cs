using MyApp.Domain.Entities;
using MyApp.Domain.ValueObjects;

namespace MyApp.Application.Interfaces;

public interface IBanRepository
{
    Task<List<Ban>> Get(CancellationToken ct);
    Task<Ban?> GetById(BanId id, CancellationToken ct);
    Task<Ban> Add(Ban entity, CancellationToken ct);
    Task Delete(Ban entity, CancellationToken ct);
    Task Update(Ban entity, CancellationToken ct);
    Task<List<Ban>> GetActiveByUserId(UserId userId, CancellationToken ct);

}