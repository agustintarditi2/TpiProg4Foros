
using MyApp.Application.Models;
using MyApp.Application.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace MyApp.Web.Controllers;

[Route("api/auth")]
[ApiController]

public class AuthController : ControllerBase
{
    private readonly AuthServices _service;
    public AuthController(AuthServices service)
    {
        _service = service;
    }

    [HttpPost("login")]
    public async Task<ActionResult<string>> Authenticate(LoginUserDTO loginDTO, CancellationToken ct)
    {
        string token = await _service.Authenticate(loginDTO, ct);
        return Ok(token);
    }
    
}