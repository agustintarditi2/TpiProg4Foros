using MyApp.Domain.ValueObjects;
using MyApp.Domain.Exceptions;
using MyApp.Domain.Entities;
using MyApp.Application.Interfaces;
using MyApp.Application.Abstractions;
using MyApp.Application.Models;
using System.Diagnostics;

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

    public async Task<UserId> CreateUser(CreateUserDTO userDTO, CancellationToken ct)
    {
        if (await _users.EmailExists(EmailAddress.Create(userDTO.Email)))
            throw new System.ApplicationException("Email is already registered.");

        var hash = _hasher.Hash(userDTO.PlainPassword);   

        var user = new User(
            userDTO.Name,
            userDTO.DateOfBirth,
            userDTO.Email,
            hash,
            _clock.UtcNow);

        await _users.Add(user, ct);
        return user.Id;
    }
    public async Task<UserDTO> GetById(Guid id, CancellationToken ct)
    {
        User? foundUser = await _users.GetById((UserId)id, ct);
        if (foundUser is null)
            throw new ApplicationServiceException("Requested user not found");
        return UserDTO.Create(foundUser);
    }

    public async Task<List<UserDTO>> Get(CancellationToken ct)
    {
        List<User>? userList = await _users.Get(ct);
        if (userList.Count < 1)
            throw new ApplicationServiceException("No users found");
        return UserDTO.CreateList(userList);
    }

    public async Task ChangeEmail(Guid id, string newEmail, CancellationToken ct)
    {
        if (await _users.EmailExists(EmailAddress.Create(newEmail)))
            throw new ApplicationServiceException("An account for this email address already exists.");
        User? originalUser = await _users.GetById((UserId)id, ct);
        if (originalUser is null)
            throw new ApplicationServiceException("Requested user not found");
        originalUser.ChangeEmail(newEmail);
        _users.Update(originalUser, ct);
    }

    
}