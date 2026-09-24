using Microsoft.AspNetCore.Mvc;
using MyApp.Application.Interfaces;
using MyApp.Domain.Entities;

namespace MyApp.Web.UserController;

[ApiController]

[Route("[controller]")]

public class UserController : ControllerBase
{
    private IUserRepository _userRepository;
    public UserController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet]
    public async Task<ActionResult<List<User>>> Get()
    {
        var users = await _userRepository.Get();
        return Ok(users);
    }
    
}