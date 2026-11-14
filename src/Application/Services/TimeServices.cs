using System.Text.RegularExpressions;
using MyApp.Application.Abstractions;
using MyApp.Application.Models;
using MyApp.Domain.Exceptions;

namespace MyApp.Application.Services;

public sealed class TimeServices
{
    // Ids de zona IANA: segmentos separados por '/', con letras, números, '_', '+' o '-'.
    // Ejemplos: Asia/Tokyo, America/Argentina/Buenos_Aires, Etc/GMT+3, UTC
    private static readonly Regex ZonePattern =
        new(@"^[A-Za-z0-9_+\-]+(/[A-Za-z0-9_+\-]+)*$", RegexOptions.Compiled);

    private readonly IWorldClock _worldClock;

    public TimeServices(IWorldClock worldClock)
    {
        _worldClock = worldClock;
    }

    public async Task<ZonedTimeDTO> GetCurrentTime(string timeZone, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(timeZone))
            throw new DomainException("Time zone is required.");

        string zone = timeZone.Trim();

        if (zone.Length > 64 || !ZonePattern.IsMatch(zone))
            throw new DomainException(
                "Time zone format is invalid. Expected something like 'Asia/Tokyo'.");

        return await _worldClock.GetCurrentTimeAsync(zone, ct);
    }
}