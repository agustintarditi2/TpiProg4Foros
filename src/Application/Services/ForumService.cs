using Application.Interfaces;
using Application.Models;
using Application.Models.Requests;
using Domain.Entities;

namespace Application.Services;

// implementa las operaciones relacionadas con los foros
public class ForumService : IForumService
{
    // repositorio utilizado para acceder a los datos de los foros
    private readonly IForumRepository _forumRepository;

    // recibe el repositorio mediante inyeccion de dependencias
    public ForumService(IForumRepository forumRepository)
    {
        _forumRepository = forumRepository;
    }

    // obtiene todos los foros registrados
    public async Task<List<ForumDto>> GetAllForumsAsync()
    {
        var forums = await _forumRepository.GetAllAsync();

        return ForumDto.CreateList(forums);
    }

    // obtiene un foro por su id
    public async Task<ForumDto?> GetForumByIdAsync(int id)
    {
        var forum = await _forumRepository.GetByIdAsync(id);

        // si el foro no existe devuelve null
        if (forum == null)
        {
            return null;
        }

        return ForumDto.Create(forum);
    }

    // crea un nuevo foro y lo guarda en la base de datos
    public async Task<ForumDto> CreateForumAsync(CreateForumRequest request)
    {
        var forum = new Forum(
            request.Title,
            request.Description
        );

        await _forumRepository.AddAsync(forum);

        return ForumDto.Create(forum);
    }

    // busca un foro por id y modifica sus datos
    public async Task<ForumDto?> UpdateForumAsync(
        int id,
        UpdateForumRequest request)
    {
        var forum = await _forumRepository.GetByIdAsync(id);

        // si el foro no existe devuelve null
        if (forum == null)
        {
            return null;
        }

        forum.Update(
            request.Title,
            request.Description
        );

        await _forumRepository.UpdateAsync(forum);

        return ForumDto.Create(forum);
    }

    // busca un foro por id y lo elimina de la base de datos
    public async Task<bool> DeleteForumAsync(int id)
    {
        var forum = await _forumRepository.GetByIdAsync(id);

        // si el foro no existe devuelve false
        if (forum == null)
        {
            return false;
        }

        await _forumRepository.DeleteAsync(forum);

        return true;
    }
}