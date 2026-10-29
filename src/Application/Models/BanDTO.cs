// Este DTO es para uso general, no tiene un caso de uso específico
using MyApp.Domain.Entities;

namespace MyApp.Application.Models;

public record BanDTO(
    Guid Id,
    Guid UserId,
    Guid AdminId,
    DateTime StartDate,
    DateTime EndDate,
    int Duration,
    string Reason,
    string Status
    )
{
    public static BanDTO Create(Ban entity)
    {
        return new BanDTO(
            entity.Id,
            entity.UserId,
            entity.AdminId,
            entity.StartDate,
            entity.EndDate,
            entity.Duration,
            entity.Reason,
            entity.Status.ToString()
        );
    }

    public static List<BanDTO> CreateList(IEnumerable<Ban> entities)
    {
        return entities.Select(entity => Create(entity)).ToList();
    }
}