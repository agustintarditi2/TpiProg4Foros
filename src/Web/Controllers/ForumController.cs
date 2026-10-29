using Microsoft.AspNetCore.Mvc;
using MyApp.Application.Models;
using MyApp.Application.Services;

namespace MyApp.Web.Controllers;

// maneja las solicitudes http relacionadas con los foros
[ApiController]
[Route("[controller]")]
public class ForumController : ControllerBase
{
    // servicio para manejar los casos de uso de foros
    private readonly ForumServices _services;

    // recibe el servicio mediante inyeccion de dependencias
    public ForumController(ForumServices services)
    {
        _services = services;
    }

    // obtiene todos los foros
    [HttpGet]
    public async Task<ActionResult<List<ForumDTO>>> Get(
        CancellationToken ct)
    {
        var forums = await _services.Get(ct);

        return Ok(forums);
    }

    // obtiene un foro por su id
    [HttpGet("{id}")]
    public async Task<ActionResult<ForumDTO>> GetById(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var forum = await _services.GetById(id, ct);

        if (forum is null)
        {
            return NotFound();
        }

        return Ok(forum);
    }

    // crea un nuevo foro
    [HttpPost]
    public async Task<ActionResult<ForumDTO>> Create(
        [FromBody] CreateForumDTO dto,
        CancellationToken ct)
    {
        var forum = await _services.Create(dto, ct);

        return CreatedAtAction(
            nameof(GetById),
            new { id = forum.Id },
            forum
        );
    }

    // modifica un foro 
    [HttpPut("{id}")]
    public async Task<ActionResult<ForumDTO>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateForumDTO dto,
        CancellationToken ct)
    {
        var forum = await _services.Update(id, dto, ct);

        if (forum is null)
        {
            return NotFound();
        }

        return Ok(forum);
    }

    // elimina un foro
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var deleted = await _services.Delete(id, ct);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}