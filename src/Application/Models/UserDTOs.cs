// Este DTO es para uso general, no tiene un caso de uso específico
using MyApp.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace MyApp.Application.Models;

public record UserDTO(
    string Name,
    DateOnly DateOfBirth,
    [EmailAddress]
    string Email, //Es seguro exponer el mail del usuario en el DTO?
    string UserRole,
    DateTimeOffset DateCreated,
    Guid UserId
    )
{
    public static UserDTO Create( User entity )
    {
        return new UserDTO(
            entity.Name,
            entity.DateOfBirth,
            entity.Email.ToString(),
            entity.Role.ToString(),
            entity.DateCreated,
            entity.Id
        );
        
    }
    public static List<UserDTO> CreateList(IEnumerable<User> entities)
    {
        return entities.Select(entity => Create(entity)).ToList();
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

    