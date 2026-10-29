namespace MyApp.Application.Models;

public sealed record CreateBanDTO(
    Guid UserId,
    Guid AdminId,
    int Days,
    string Reason);
