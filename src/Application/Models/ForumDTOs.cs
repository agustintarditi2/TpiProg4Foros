using MyApp.Domain.Entities;

namespace MyApp.Application.Models;

// datos de un foro que se devuelven desde la api
public record ForumDTO(
    Guid Id,
    string Title,
    string Description,
    bool State
)
{
    // convierte una entidad forum en un dto
    public static ForumDTO Create(Forum entity)
    {
        return new ForumDTO(
            entity.Id,
            entity.Title,
            entity.Description,
            entity.State
        );
    }

    // convierte una lista de entidades forum en una lista de dto
    public static List<ForumDTO> CreateList(IEnumerable<Forum> entities)
    {
        return entities.Select(Create).ToList();
    }
}

// datos necesarios para crear un foro
public sealed record CreateForumDTO(
    string Title,
    string Description
);

// datos necesarios para modificar un foro
public sealed record UpdateForumDTO(
    string Title,
    string Description
);