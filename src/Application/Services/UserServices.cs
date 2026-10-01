using MyApp.Domain.ValueObjects;
using MyApp.Domain.Exceptions;
using MyApp.Domain.Entities;
using MyApp.Application.Interfaces;
using MyApp.Application.Abstractions;
using MyApp.Application.Models;

namespace MyApp.Application.Services;

public sealed class UserServices
{
    private readonly IPasswordHasher _hasher;
    private readonly IUserRepository _users;
    private readonly IClock _clock;

    public UserServices(
        IPasswordHasher hasher,
        IUserRepository users,
        IClock clock)
    {
        _hasher = hasher;
        _users = users;
        _clock = clock;
    }

    public async Task<UserId> CreateUser(CreateUserDTO userDTO)
    {
        if (await _users.EmailExists(EmailAddress.Create(userDTO.Email)))
            throw new ApplicationException("Email is already registered.");

        var hash = _hasher.Hash(userDTO.PlainPassword);   

        var user = new User(
            userDTO.Name,
            userDTO.DateOfBirth,
            userDTO.Email,
            hash,
            _clock.UtcNow);

        await _users.Add(user);
        return user.Id;
    }
    
}