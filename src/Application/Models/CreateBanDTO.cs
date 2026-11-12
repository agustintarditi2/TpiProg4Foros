namespace MyApp.Application.Models;

public sealed record CreateBanDTO(
    Guid UserId,
    int Days,
    string Reason);