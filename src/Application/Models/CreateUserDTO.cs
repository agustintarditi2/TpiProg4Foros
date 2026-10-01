namespace MyApp.Application.Models;

public sealed record CreateUserDTO(
    string Name,
    DateOnly DateOfBirth,
    string Email,
    string PlainPassword);