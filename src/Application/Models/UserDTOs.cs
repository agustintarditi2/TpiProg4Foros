// Este DTO es para uso general, no tiene un caso de uso específico
using MyApp.Application.Abstractions;
using MyApp.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace MyApp.Application.Models;

public record UserDTO(
    string Name,
    int Age,
    [EmailAddress]
    string Email, //Es seguro exponer el mail del usuario en el DTO?
    string UserRole,
    DateTimeOffset DateCreated,
    Guid UserId
    )
{
    public static UserDTO Create( User entity, DateTimeOffset now )
    {
        DateOnly today = DateOnly.FromDateTime(now.DateTime);
        DateOnly birthDate = entity.DateOfBirth;
        int age = today.Year - birthDate.Year;
        if (today < birthDate.AddYears(age)) age--;
        return new UserDTO(
            entity.Name,
            age,
            entity.Email.ToString(),
            entity.Role.ToString(),
            entity.DateCreated,
            entity.Id
        );
        
    }
    public static List<UserDTO> CreateList(IEnumerable<User> entities, DateTimeOffset now)
    {
        return entities.Select(entity => Create(entity, now)).ToList();
    }
}

// ======================================================================

public sealed record PatchUserDTO(
    string? Name,
    DateOnly? DateOfBirth,
    string? Email,
    string? PlainPassword,
    int? Role,
    Guid Id);




// ======================================================================



public sealed record CreateUserDTO(
    string Name,
    DateOnly DateOfBirth,
    string Email,
    string PlainPassword);



// ======================================================================

public sealed record LoginUserDTO(
    string Email,
    string PlainPassword);