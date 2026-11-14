using Microsoft.AspNetCore.Mvc;
using MyApp.Application.Models;
using MyApp.Application.Services;

namespace MyApp.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class TimeController : ControllerBase
{
    private readonly TimeServices _services;

    public TimeController(TimeServices services)
    {
        _services = services;
    }

    [HttpGet]
    public async Task<ActionResult<ZonedTimeDTO>> GetCurrentTime(
        [FromQuery] string? timeZone,
        CancellationToken cancellationToken)
    {
        var time = await _services.GetCurrentTime(timeZone ?? string.Empty, cancellationToken);
        return Ok(time);
    }
}