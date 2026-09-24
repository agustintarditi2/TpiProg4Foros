namespace MyApp.Application.Users;

public sealed record RegisterUserCommand(
    string Name,
    DateOnly DateOfBirth,
    string Email,
    string PlainPassword);