using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using MyApp.Application.Abstractions;
using MyApp.Application.Models;
using MyApp.Domain.Exceptions;

namespace MyApp.Infrastructure.Services;

public sealed class TimeApiClient : IWorldClock
{
    private readonly HttpClient _http;
    private readonly ILogger<TimeApiClient> _logger;

    public TimeApiClient(HttpClient http, ILogger<TimeApiClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<ZonedTimeDTO> GetCurrentTimeAsync(string timeZone, CancellationToken ct)
    {
        string url = $"api/time/current/zone?timeZone={Uri.EscapeDataString(timeZone)}";

        try
        {
            using HttpResponseMessage response = await _http.GetAsync(url, ct);

            if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.NotFound)
                throw new NotFoundException($"Time zone '{timeZone}' was not found.");

            response.EnsureSuccessStatusCode();

            TimeApiResponse? body =
                await response.Content.ReadFromJsonAsync<TimeApiResponse>(cancellationToken: ct);

            if (body?.LocalDateTime is null || body.Zone is null)
                throw new ExternalServiceException("The time service returned an unexpected response.");

            return new ZonedTimeDTO(body.Zone, body.LocalDateTime.Value);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Time service request failed for zone {Zone}", timeZone);
            throw new ExternalServiceException("The time service is unavailable.", ex);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            _logger.LogError(ex, "Time service timed out for zone {Zone}", timeZone);
            throw new ExternalServiceException("The time service timed out.", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Time service returned unreadable JSON for zone {Zone}", timeZone);
            throw new ExternalServiceException("The time service returned an unreadable response.", ex);
        }
    }

    private sealed record TimeApiResponse(
        [property: JsonPropertyName("dateTime")] DateTime? LocalDateTime,
        [property: JsonPropertyName("timeZone")] string? Zone);
}