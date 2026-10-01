using Microsoft.AspNetCore.Mvc;
using MyApp.Application.Services;
using MyApp.Application.Models;
using MyApp.Domain.Entities;
using MyApp.Domain.ValueObjects;
using MyApp.Web.Contracts;

namespace MyApp.Web.UserController;

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
    public async Task<ActionResult<List<User>>> Get()
    {
        var users = await _services.Get();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetById([FromRoute] Guid id)
    {
        var user = await _services.GetById(id);
        return Ok(user);
    }
    [HttpPost]
    public async Task<ActionResult<UserId>> RegisterUser(RegisterUserDTO sentUser)
    {
        var newUser = new CreateUserDTO(
            sentUser.Name,
            sentUser.DateOfBirth,
            sentUser.Email,
            sentUser.PlainPassword);
        var registeredUserId = await _services.CreateUser(newUser);
        return Ok(registeredUserId);
    }
    
}