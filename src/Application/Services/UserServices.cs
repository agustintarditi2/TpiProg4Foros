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

    public async Task UpdateUser(PatchUserDTO user, CancellationToken ct)
    {
// Verificamos que haya un ID de usuario en el DTO
        if (user.Id is null)
            throw new ApplicationServiceException("ID is required to modify a user.")
// Verificamos que al menos un campo tenga contenido.
        if (user.Email is null && user.Name is null && user.DateOfBirth is null && user.PlainPassword is null && user.Role is null)
            return 
// De no ser así, devolvemos un hermoso 204 sin contenido, sin molestar a la base de datos ni gastar tiempo de procesamiento.

// Invocamos al usuario original
        var userId = (UserId)user.Id;
        User? changedUser = await _users.GetById(userId, ct);
// Ahora changedUser existe en este namespace y lo podemos modificar para después devolverlo con un único método en el repositorio.
    if (changedUser is null)
        throw new ApplicationServiceException("Requested user not found");
// Si el usuario no existe o no tiene datos guardados, hay error.

// Sigue el use case de cambiar el mail
        if (user.Email is not null)
            changedUser = ChangeEmail(user.Email, changedUser);
// Sigue el use case de cambiar el nombre
        if (user.Name is not null)
            changedUser = ChangeName(user.Name, changedUser);
// Sigue el use case de cambiar la fecha de nacimiento
        if (user.DateOfBirth is not null)
            changedUser = ChangeDoB(user.DateOfBirth, changedUser)
        _users.Update(changedUser, ct);
    }
// Métodos que se llaman para modificar al usuario que después devuelve UpdateUser()
    internal async Task<User> ChangeEmail(string email, User user)
    {
        if (await _users.EmailExists(EmailAddress.Create(email)))
            throw new ApplicationServiceException("An account for this email address already exists.");
        user.ChangeEmail(newEmail);
        return user;
    }
    internal async Task<User> ChangeName(string name, User user)
    {
        user.Rename(name);
        return user;
    }
    internal async Task<User> ChangeDoB(DateOnly dob, User user)
    {
        DateOnly today = DateOnly.FromDateTime(_clock.UtcNow.Date)
        if (dob > today)
            throw new ApplicationServiceException("Date of birth cannot be in the future.");
        //user.(newEmail);
        return user;
    }
    
}