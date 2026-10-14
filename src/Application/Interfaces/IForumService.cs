using Application.Models;
using Application.Models.Requests;

namespace Application.Interfaces;

// define las operaciones disponibles para trabajar con foros
public interface IForumService
{
    // obtiene todos los foros registrados
    Task<List<ForumDto>> GetAllForumsAsync();

    // obtiene un foro por su id
    Task<ForumDto?> GetForumByIdAsync(int id);

    // crea un nuevo foro
    Task<ForumDto> CreateForumAsync(CreateForumRequest request);

    // modifica un foro existente
    Task<ForumDto?> UpdateForumAsync(
        int id,
        UpdateForumRequest request);

    // elimina un foro por su id
    Task<bool> DeleteForumAsync(int id);
}