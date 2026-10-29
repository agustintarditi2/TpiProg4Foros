using MyApp.Application.Interfaces;
using MyApp.Application.Models;
using MyApp.Domain.Entities;
using MyApp.Domain.ValueObjects;

namespace MyApp.Application.Services;

// implementa los casos de uso relacionados con los foros
public sealed class ForumServices
{
    // repositorio para acceder a los datos de los foros
    private readonly IForumRepository _repository;

    // recibe el repositorio por inyeccion de dependencias
    public ForumServices(IForumRepository repository)
    {
        _repository = repository;
    }

    // obtiene todos los foros
    public async Task<List<ForumDTO>> Get(CancellationToken ct)
    {
        var forums = await _repository.Get(ct);

        return ForumDTO.CreateList(forums);
    }

    // obtiene un foro por su id
    public async Task<ForumDTO?> GetById(Guid id, CancellationToken ct)
    {
        var forum = await _repository.GetById((ForumId)id, ct);

        // si el foro no existe devuelve null
        if (forum is null)
        {
            return null;
        }

        return ForumDTO.Create(forum);
    }

    // crea un nuevo foro
    public async Task<ForumDTO> Create(
        CreateForumDTO dto,
        CancellationToken ct)
    {
        var forum = new Forum(
            dto.Title,
            dto.Description
        );

        var addedForum = await _repository.Add(forum, ct);

        return ForumDTO.Create(addedForum);
    }

    // modifica un foro
    public async Task<ForumDTO?> Update(
        Guid id,
        UpdateForumDTO dto,
        CancellationToken ct)
    {
        var forum = await _repository.GetById((ForumId)id, ct);

        // si el foro no existe devuelve null
        if (forum is null)
        {
            return null;
        }

        forum.Update(
            dto.Title,
            dto.Description
        );

        await _repository.Update(forum, ct);

        return ForumDTO.Create(forum);
    }

    // elimina un foro por su id
    public async Task<bool> Delete(
        Guid id,
        CancellationToken ct)
    {
        var forum = await _repository.GetById((ForumId)id, ct);

        // si el foro no existe devuelve false
        if (forum is null)
        {
            return false;
        }

        await _repository.Delete(forum, ct);

        return true;
    }
}