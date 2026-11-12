using Microsoft.AspNetCore.Mvc;
using MyApp.Application.Services;
using MyApp.Application.Models;
using MyApp.Domain.Entities;
using MyApp.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using MyApp.Web.Extensions;

namespace MyApp.Web.Controllers;

[ApiController]

[Route("[controller]")]

public class UserController : ControllerBase
{
    private readonly UserServices _services;
    public UserController(UserServices services)
    {
        _services = services;
    }

    [HttpGet]
    public async Task<ActionResult<List<RestrictedUserDTO>>> Get(CancellationToken cancellationToken)
    {
        var users = await _services.Get(cancellationToken);
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RestrictedUserDTO>> GetRestrictedById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var user = await _services.GetRestrictedById(id, cancellationToken);
        return Ok(user);
    }

    [Authorize(Roles ="Sysadmin, Admin")]
    [HttpGet("{id}/full")]
    public async Task<ActionResult<UserDTO>> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var user = await _services.GetById(id, cancellationToken);
        return Ok(user);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDTO>> GetSelf([FromHeader] string token, CancellationToken cancellationToken)
    {
        Guid userId = User.GetAuthenticatedUserId();
        var user = await _services.GetById(userId, cancellationToken);
        return Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<UserId>> RegisterUser(CreateUserDTO sentUser, CancellationToken cancellationToken)
    {
        var registeredUserId = await _services.CreateUser(sentUser, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = registeredUserId },          registeredUserId);
    }

    [Authorize]
    [HttpPatch]
    public async Task<IActionResult> UpdateUser(PatchUserDTO userDTO, CancellationToken ct)
    {
        Guid userId = User.GetAuthenticatedUserId();
        await _services.UpdateUser(userDTO, userId, ct);
        return NoContent();
    }

    [Authorize(Roles = "SysAdmin")]
    [HttpPatch("{id:guid}/role")]
    public async Task<IActionResult> ChangeUserRole(
        [FromRoute] Guid id,
        [FromBody] ChangeUserRoleDTO dto,
        CancellationToken ct)
    {
        // identifica al SysAdmin por el JWT
        Guid adminId = User.GetAuthenticatedUserId();

        await _services.ChangeUserRole(
            id, dto.Role, adminId, ct);

        return NoContent();

    }
    //El usuario no puede seleccionar que user eliminar, el id se obtiene del JWT, solo se puede elimnar a si mismo
    [Authorize]
    [HttpDelete("me")]
    public async Task<IActionResult> DeleteUser(CancellationToken ct)
    {
        // se identifica al usuario que quiere eliminar su cuenta
        Guid userId = User.GetAuthenticatedUserId();
        
        await _services.DeleteOwnAccount(userId, ct);
        return NoContent();
    }
}