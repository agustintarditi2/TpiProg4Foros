namespace MyApp.Application.Models;

public sealed record PatchUserDTO(
    string? Name,
    DateOnly? DateOfBirth,
    string? Email,
    string? PlainPassword,
    string? Role,
    Guid Id);