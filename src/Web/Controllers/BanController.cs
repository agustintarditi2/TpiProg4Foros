using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.Application.Services;
using MyApp.Application.Models;
using MyApp.Domain.ValueObjects;
using System.Security.Claims;

namespace MyApp.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class BanController : ControllerBase
{
    private readonly BanServices _services;
    public BanController(BanServices services)
    {
        _services = services;
    }
    [Authorize(Roles = "Admin,SysAdmin")]
    [HttpGet]
    public async Task<ActionResult<List<BanDTO>>> Get(CancellationToken cancellationToken)
    {
        var bans = await _services.Get(cancellationToken);
        return Ok(bans);
    }
    [Authorize(Roles = "Admin,SysAdmin")]
    [HttpGet("{id}")]
    public async Task<ActionResult<BanDTO>> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var ban = await _services.GetById(id, cancellationToken);
        return Ok(ban);
    }

    [Authorize(Roles = "Admin,SysAdmin")]
    [HttpPost]
    public async Task<ActionResult<BanId>> CreateBan(CreateBanDTO banDTO, CancellationToken cancellationToken)
    {
        var adminIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(adminIdClaim, out var adminId))
            return Unauthorized();

        var createdBanId = await _services.CreateBan(banDTO, adminId, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = createdBanId }, createdBanId);
    }

    [Authorize(Roles = "Admin,SysAdmin")]
    [HttpPatch]
    public async Task<IActionResult> ModifyBan(ModifyBanDTO banDTO, CancellationToken ct)
    {
        await _services.ModifyBan(banDTO, ct);
        return NoContent();
    }

    [Authorize(Roles = "Admin,SysAdmin")]
    [HttpPatch("{id}/revoke")]
    public async Task<IActionResult> Desban([FromRoute] Guid id, CancellationToken ct)
    {
        await _services.Desban(id, ct);
        return NoContent();
    }

    [Authorize(Roles = "Admin,SysAdmin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBan([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        await _services.DeleteBan(id, cancellationToken);
        return NoContent();
    }
}