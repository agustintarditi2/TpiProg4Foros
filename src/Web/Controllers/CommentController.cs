
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Application.Models;
using MyApp.Application.Services;
using MyApp.Web.Extensions;

namespace MyApp.Web.Controllers;

[ApiController]
[Route("[controller]")]

public class CommentController : ControllerBase
{
    

    private readonly CommentServices _services;

    public CommentController(CommentServices commentServices)
    {
        _services = commentServices;
    }

    [HttpGet]

    public async Task<ActionResult<List<CommentDTO>>> GetByPost([FromQuery] Guid postId, CancellationToken ct)
    {
        return Ok(await _services.GetByPost(postId, ct));
    }

    [HttpGet("by-poster")]
    public async Task<ActionResult<List<CommentDTO>>> GetByPoster([FromQuery] Guid userId, CancellationToken ct)
    {
        return Ok(await _services.GetByPoster(userId, ct));
    }

    [HttpGet("by-thread")]
    public async Task<ActionResult<List<CommentDTO>>> GetByThread([FromQuery] Guid commentId, CancellationToken ct)
    {
        return Ok(await _services.GetByThread(commentId, ct));
    }


    [HttpGet("{id}")]

    public async Task<ActionResult<CommentDTO>> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        return Ok(await _services.GetById(id, ct));
    }


    [Authorize]
    [HttpPost]
    public async Task<ActionResult<CommentDTO>> NewComment([FromBody] CreateCommentDTO comment, CancellationToken ct)
    {
        Guid userId = User.GetAuthenticatedUserId();

        var newComment = await _services.Create(comment, userId, ct);

        return CreatedAtAction(
            nameof(GetById),
            new { id = newComment.id },
            newComment
        );
    }
    
    [Authorize]
    [HttpPatch("{id:guid}")]

    public async Task<ActionResult> EditComment([FromRoute] Guid id, [FromBody] string newBody, CancellationToken ct)
    {   
        Guid userId = User.GetAuthenticatedUserId();
        await _services.Update(id, newBody,userId, ct);
        return NoContent();
    }
    
    [Authorize(Roles = "SysAdmin, Admin")]
    [HttpPatch("{id:guid}/suppress")]

    public async Task<ActionResult> SuppressComment([FromRoute] Guid id, [FromBody] bool hidden, CancellationToken ct)
    {
        await _services.HideOrUnhide(id, hidden, ct);
        return NoContent();
    }
    
    [Authorize]
    [HttpPatch("{id:guid}/visible")]

    public async Task<ActionResult> HideOrUnhideComment([FromRoute] Guid id, [FromBody] bool hidden, CancellationToken ct)
    {
        Guid userId = User.GetAuthenticatedUserId();
        await _services.HideOrUnhideSelf(id, userId, hidden, ct);
        return NoContent();
    }
    
    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteComment([FromRoute] Guid id, CancellationToken ct)
    {
        Guid userId = User.GetAuthenticatedUserId();
        await _services.Delete(id,userId, ct);
        return NoContent();
    }
}