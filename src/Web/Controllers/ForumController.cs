using Application.Interfaces;
using Application.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ForumController : ControllerBase
{
    // variable para guardar el servicio de foros que sera inyectado en el constructor
    private readonly IForumService _forumService;

    // recibe el servicio mediante inyeccion de dependencias
    public ForumController(IForumService forumService)
    {
        // lo guarda en _forumService para usarlo en el controller
        _forumService = forumService;
    }

    // crea un nuevo foro
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateForumRequest request)
    {
        var forum = await _forumService.CreateForumAsync(request);

        return Ok(forum);
    }

    // obtiene todos los foros registrados
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var forums = await _forumService.GetAllForumsAsync();

        return Ok(forums);
    }

    // obtiene un foro por su id
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var forum = await _forumService.GetForumByIdAsync(id);

        // si no existe devuelve 404
        if (forum == null)
        {
            return NotFound();
        }

        return Ok(forum);
    }

    // modifica un foro existente
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateForumRequest request)
    {
        var forum = await _forumService.UpdateForumAsync(
            id,
            request);

        // si no existe devuelve 404
        if (forum == null)
        {
            return NotFound();
        }

        return Ok(forum);
    }

    // elimina un foro por su id
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _forumService.DeleteForumAsync(id);

        // si no existe devuelve 404
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}