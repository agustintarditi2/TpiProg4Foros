namespace MyApp.Application.Models;

public sealed record ModifyBanDTO(
    Guid Id,
    int NewDays,
    string NewReason);