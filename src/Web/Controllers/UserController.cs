using Microsoft.AspNetCore.Mvc;
using MyApp.Application.Services;
using MyApp.Application.Models;
using MyApp.Domain.Entities;
using MyApp.Domain.ValueObjects;

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
    public async Task<ActionResult<List<User>>> Get(CancellationToken cancellationToken)
    {
        var users = await _services.Get(cancellationToken);
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var user = await _services.GetById(id, cancellationToken);
        return Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<UserId>> RegisterUser(CreateUserDTO sentUser, CancellationToken cancellationToken)
    {
        var registeredUserId = await _services.CreateUser(sentUser, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = registeredUserId },          registeredUserId);
    }

    [HttpPatch]
    public async Task<IActionResult> UpdateUser(PatchUserDTO userDTO, CancellationToken ct)
    {
        await _services.UpdateUser(userDTO, ct);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        await _services.DeleteUser(id, cancellationToken);
        return NoContent();
    }
}