
using MyApp.Application.Models;
using MyApp.Application.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace MyApp.Web.Controllers;

[Route("[controller]")]
[ApiController]

public class AuthController : ControllerBase
{
    private readonly AuthServices _service;
    public AuthController(AuthServices service)
    {
        _service = service;
    }

    [HttpPost("auth")]
    public async Task<ActionResult<string>> Authenticate(LoginUserDTO loginDTO)
    {
        string token = await _service.Authenticate(loginDTO);
        return Ok(token);
    }
    
}