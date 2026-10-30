
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Application.Models;
using MyApp.Application.Services;
using MyApp.Domain.Entities;
using MyApp.Domain.ValueObjects;

namespace MyApp.Web.Controllers;

[ApiController]
[Route("[controller]")]

public class PostController : ControllerBase
{
    

    private readonly PostServices _services;

    public PostController(PostServices postServices)
    {
        _services = postServices;
    }

    [HttpGet]

    public async Task<ActionResult<List<PostDTO>>> GetByForum([FromQuery] Guid forumId, CancellationToken ct)
    {
        return Ok(await _services.GetByForum(forumId, ct));
    }

    [HttpGet("by-poster")]
    public async Task<ActionResult<List<PostDTO>>> GetByPoster([FromQuery] Guid userId, CancellationToken ct)
    {
        return Ok(await _services.GetByPoster(userId, ct));
    }


    [HttpGet("{id}")]

    public async Task<ActionResult<PostDTO>> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        return Ok(await _services.GetById(id, ct));
    }


    [Authorize]
    [HttpPost]
    public async Task<ActionResult<PostDTO>> NewPost([FromBody] CreatePostDTO post, CancellationToken ct)
    {
    // Obtiene el identificador del usuario autenticado
    string? userClaim = User.FindFirst("user")?.Value;

    if (!Guid.TryParse(userClaim, out Guid userId) || userId == Guid.Empty)
        return Unauthorized();

    var newPost = await _services.Create(post, userId, ct);

    return CreatedAtAction(
        nameof(GetById),
        new { id = newPost.id },
        newPost
    );
}
    [Authorize]
    [HttpPatch("{id:guid}")]

    public async Task<ActionResult> EditPost([FromRoute] Guid id, [FromBody] string newBody, CancellationToken ct)
    {
        await _services.Update(id, newBody, ct);
        return NoContent();
    }
    
    [Authorize]
    [HttpPatch("{id:guid}/visibility")]

    public async Task<ActionResult> HideOrUnhidePost([FromRoute] Guid id, [FromBody] bool hidden, CancellationToken ct)
    {
        await _services.HideOrUnhide(id, hidden, ct);
        return NoContent();
    }
    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeletePost([FromRoute] Guid id, CancellationToken ct)
    {
        await _services.Delete((PostId)id, ct);
        return NoContent();
    }
}