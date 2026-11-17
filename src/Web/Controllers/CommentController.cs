
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
    private readonly ReactionServices _reactionServices;

    public CommentController(CommentServices commentServices,ReactionServices reactionServices)
    {
        _services = commentServices;
        _reactionServices = reactionServices;
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
        await _services.Update(id, newBody, userId, ct);
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

    public async Task<ActionResult> HideComment([FromRoute] Guid id, [FromBody] bool hidden, CancellationToken ct)
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
        await _services.Delete(id, userId, ct);
        return NoContent();
    }

    [Authorize]
    [HttpPut("{id:guid}/reaction")]
    public async Task<IActionResult> SetCommentReaction(
        [FromRoute] Guid id,
        [FromBody] bool isLike,
        CancellationToken ct)
    {
        // obtiene el usuario autenticado
        Guid userId = User.GetAuthenticatedUserId();

        // agrega o cambia la reaccion del comentario
        await _reactionServices.SetCommentReaction(
            id, userId, isLike, ct);

        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id:guid}/reaction")]
    public async Task<IActionResult> RemoveCommentReaction(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        // obtiene el usuario autenticado
        Guid userId = User.GetAuthenticatedUserId();

        // elimina la reaccion del comentario
        await _reactionServices.RemoveCommentReaction(
            id, userId, ct);

        return NoContent();
    }
}