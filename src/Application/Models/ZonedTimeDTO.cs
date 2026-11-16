namespace MyApp.Application.Models;

public sealed record ZonedTimeDTO(string TimeZone, DateTime LocalDateTime);