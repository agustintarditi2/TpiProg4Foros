
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

    public async Task<ActionResult<List<PostDTO>>> GetByForum([FromQuery] ForumId forumId, CancellationToken ct)
    {
        return Ok(await _services.GetByForum(forumId, ct));
    }

    [HttpGet]

    public async Task<ActionResult<List<PostDTO>>> GetByPoster([FromQuery] UserId userId, CancellationToken ct)
    {
        return Ok(await _services.GetByPoster(userId, ct));
    }


    [HttpGet("{id}")]

    public async Task<ActionResult<PostDTO>> GetById([FromRoute] PostId id, CancellationToken ct)
    {
        return Ok(await _services.GetById(id, ct));
    }

    [HttpPost]

    public async Task<ActionResult<PostDTO>> NewPost([FromBody] PostDTO post, CancellationToken ct)
    {
        PostDTO newPost = await _services.Create(post, ct);
        return CreatedAtAction(nameof(NewPost), newPost);
    }
    
    [HttpPatch("[id]")]

    public async Task<ActionResult> EditPost([FromRoute] Guid id, [FromBody] string newBody, CancellationToken ct)
    {
        await _services.Update(id, newBody, ct);
        return NoContent();
    }
    public async Task<ActionResult> HideOrUnhidePost([FromRoute] Guid id, CancellationToken ct)
    {
        await _services.HideOrUnhide((PostId) id, ct);
        return NoContent();
    }
}