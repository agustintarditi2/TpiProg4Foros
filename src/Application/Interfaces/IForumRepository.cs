using MyApp.Domain.Entities;
using MyApp.Domain.ValueObjects;

namespace MyApp.Application.Interfaces;

// operaciones necesarias para acceder a los datos de los foros
public interface IForumRepository
{
    // obtiene todos los foros
    Task<List<Forum>> Get(CancellationToken ct);

    // obtiene un foro por su id
    Task<Forum?> GetById(ForumId id, CancellationToken ct);

    // agrega un nuevo foro
    Task<Forum> Add(Forum entity, CancellationToken ct);

    // modifica un foro existente
    Task Update(Forum entity, CancellationToken ct);

    // elimina un foro
    Task Delete(Forum entity, CancellationToken ct);
}