namespace MyApp.Web.Contracts;

public sealed record RegisterUserDTO(
    string Name,
    DateOnly DateOfBirth,
    string Email,
    string PlainPassword);