using Microsoft.AspNetCore.Mvc;
using MyApp.Application.Interfaces;
using MyApp.Application.Users;
using MyApp.Domain.Entities;
using MyApp.Domain.ValueObjects;
using MyApp.Web.Contracts;

namespace MyApp.Web.UserController;

[ApiController]

[Route("[controller]")]

public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly RegisterUserHandler _registerUser;
    public UserController(IUserRepository userRepository, RegisterUserHandler registerUser)
    {
        _registerUser = registerUser;
        _userRepository = userRepository;
    }

    [HttpGet]
    public ActionResult<List<User>> Get()
    {
        var users = _userRepository.Get();
        return Ok(users);
    }
    [HttpPost]
    public async Task<ActionResult<UserId>> RegisterUser(RegisterUserDTO sentUser)
    {
        var newUser = new RegisterUserCommand(
            sentUser.Name,
            sentUser.DateOfBirth,
            sentUser.Email,
            sentUser.PlainPassword);
        var registeredUserId = await _registerUser.Handle(newUser);
        return Ok(registeredUserId);
    }
    
}