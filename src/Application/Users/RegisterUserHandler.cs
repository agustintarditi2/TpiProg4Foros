using MyApp.Domain.Entities;
using MyApp.Domain.ValueObjects;
using MyApp.Domain.Exceptions;
using MyApp.Application.Interfaces;
using MyApp.Application.Abstractions;

namespace MyApp.Application.Users;

public sealed class RegisterUserHandler
{
    private readonly IPasswordHasher _hasher;
    private readonly IUserRepository _users;
    private readonly IClock _clock;

    public RegisterUserHandler(
        IPasswordHasher hasher,
        IUserRepository users,
        IClock clock)
    {
        _hasher = hasher;
        _users = users;
        _clock = clock;
    }

    public async Task<UserId> Handle(RegisterUserCommand cmd)
    {
        if (await _users.EmailExists(EmailAddress.Create(cmd.Email)))
            throw new DomainException("Email is already registered.");

        var hash = _hasher.Hash(cmd.PlainPassword);   

        var user = new User(
            cmd.Name,
            cmd.DateOfBirth,
            cmd.Email,
            hash,
            _clock.UtcNow);

        await _users.Add(user);
        return user.Id;
    }
}