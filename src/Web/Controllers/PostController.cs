
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Application.Models;
using MyApp.Application.Services;
using MyApp.Web.Extensions;

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
        Guid userId = User.GetAuthenticatedUserId();

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
        Guid userId = User.GetAuthenticatedUserId();
        await _services.Update(id, newBody,userId, ct);
        return NoContent();
    }
    
    [Authorize(Roles = "SysAdmin, Admin")]
    [HttpPatch("{id:guid}/suppress")]

    public async Task<ActionResult> SuppressPost([FromRoute] Guid id, [FromBody] bool hidden, CancellationToken ct)
    {
        await _services.Suppress(id, hidden, ct);
        return NoContent();
    }
    
    [Authorize]
    [HttpPatch("{id:guid}/visible")]

    public async Task<ActionResult> HideOrUnhidePost([FromRoute] Guid id, [FromBody] bool hidden, CancellationToken ct)
    {
        Guid userId = User.GetAuthenticatedUserId();
        await _services.HideOrUnhideSelf(id, userId, hidden, ct);
        return NoContent();
    }
    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeletePost([FromRoute] Guid id, CancellationToken ct)
    {
        Guid userId = User.GetAuthenticatedUserId();
        await _services.Delete(id,userId, ct);
        return NoContent();
    }
}