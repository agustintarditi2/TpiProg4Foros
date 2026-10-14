using Microsoft.AspNetCore.Mvc;
using MyApp.Application.Services;
using MyApp.Application.Models;
using MyApp.Domain.ValueObjects;

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

    [HttpGet]
    public async Task<ActionResult<List<BanDTO>>> Get(CancellationToken cancellationToken)
    {
        var bans = await _services.Get(cancellationToken);
        return Ok(bans);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BanDTO>> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var ban = await _services.GetById(id, cancellationToken);
        return Ok(ban);
    }

    [HttpPost]
    public async Task<ActionResult<BanId>> CreateBan(CreateBanDTO banDTO, CancellationToken cancellationToken)
    {
        var createdBanId = await _services.CreateBan(banDTO, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = createdBanId }, createdBanId);
    }

    [HttpPatch]
    public async Task<IActionResult> ModifyBan(ModifyBanDTO banDTO, CancellationToken ct)
    {
        await _services.ModifyBan(banDTO, ct);
        return NoContent();
    }

    [HttpPatch("{id}/revoke")]
    public async Task<IActionResult> Desban([FromRoute] Guid id, CancellationToken ct)
    {
        await _services.Desban(id, ct);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBan([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        await _services.DeleteBan(id, cancellationToken);
        return NoContent();
    }
}