using MyApp.Domain.Entities;
using MyApp.Domain.ValueObjects;

namespace MyApp.Application.Interfaces;

public interface IBanRepository
{
    Task<List<Ban>> Get(CancellationToken ct);
    Task<Ban?> GetById(BanId id, CancellationToken ct);
    Task<Ban> Add(Ban entity, CancellationToken ct);
    void Delete(Ban entity, CancellationToken ct);
    void Update(Ban entity, CancellationToken ct);

}