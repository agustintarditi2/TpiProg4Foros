using MyApp.Application.Models;

namespace MyApp.Application.Abstractions;

public interface IWorldClock
{
    Task<ZonedTimeDTO> GetCurrentTimeAsync(string timeZone, CancellationToken ct);
}