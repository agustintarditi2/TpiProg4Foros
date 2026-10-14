using Domain.Entities;

namespace Application.Models;

// representa los datos de un foro que se devuelven desde la api
public record ForumDto(
    int Id,
    string Title,
    string Description,
    bool State
)
{
    // convierte una entidad forum en un dto
    public static ForumDto Create(Forum forum)
    {
        return new ForumDto(
            forum.Id,
            forum.Title,
            forum.Description,
            forum.State
        );
    }

    // convierte una lista de entidades forum en una lista de dto
    public static List<ForumDto> CreateList(IEnumerable<Forum> forums)
    {
        return forums.Select(Create).ToList();
    }
}