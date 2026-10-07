using MyApp.Domain.ValueObjects;
using MyApp.Domain.Exceptions;
using MyApp.Domain.Entities;
using MyApp.Application.Interfaces;
using MyApp.Application.Abstractions;
using MyApp.Application.Models;
using MyApp.Domain.Enums;

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


//=========================================================================


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

    
//=========================================================================



    public async Task<UserDTO> GetById(Guid id, CancellationToken ct)
    {
        User? foundUser = await _users.GetById((UserId)id, ct);
        if (foundUser is null)
            throw new ApplicationServiceException("Requested user not found");
        return UserDTO.Create(foundUser);
    }

    
//=========================================================================



    public async Task<List<UserDTO>> Get(CancellationToken ct)
    {
        List<User>? userList = await _users.Get(ct);
        if (userList.Count < 1)
            throw new ApplicationServiceException("No users found");
        return UserDTO.CreateList(userList);
    }

    
//=========================================================================



    public async Task UpdateUser(PatchUserDTO user, CancellationToken ct)
    {

// Verificamos que haya un ID de usuario en el DTO
        if (user.Id == Guid.Empty)
            throw new ApplicationServiceException("ID is required to modify a user.");

// Verificamos que al menos un campo tenga contenido.
        if (user.Email is null && user.Name is null && user.DateOfBirth is null && user.PlainPassword is null && user.Role is null)
            return ;

// De no ser así, devolvemos un hermoso 204 sin contenido, sin molestar a la base de datos ni gastar tiempo de procesamiento.

// Invocamos al usuario original
        var userId = (UserId)user.Id;
        User? changedUser = await _users.GetById(userId, ct);

// Ahora changedUser existe en este namespace y lo podemos modificar para después devolverlo con un único método en el repositorio.
    if (changedUser is null)
        throw new ApplicationServiceException("Requested user not found");

// Si el usuario no existe o no tiene datos guardados, hay error.

// Sigue el use case de cambiar el mail
        if (user.Email is string email)
            changedUser = await ChangeEmail(email, changedUser);

// Sigue el use case de cambiar el nombre
        if (user.Name is string name)
            changedUser = await ChangeName(name, changedUser);

// Sigue el use case de cambiar la fecha de nacimiento
        if (user.DateOfBirth is DateOnly dob)
            changedUser = await ChangeDoB(dob, changedUser);

// Sigue el use case de cambiar la contraseña
        if (user.PlainPassword is string plainPassword)
            changedUser = await ChangePassword(plainPassword, changedUser);
// Sigue el use case de cambiar el rol
        if (user.Role is int role)
            changedUser = await ChangeRole(role, changedUser);
// Por último, llamamos al repositorio para que haga el update de changedUser, que ya tiene los cambios aplicados.
        await _users.Update(changedUser, ct);
        return;
    }


// Métodos que se llaman para modificar al usuario que después devuelve UpdateUser()

    private async Task<User> ChangeEmail(string email, User user)
    {
        if (await _users.EmailExists(EmailAddress.Create(email)))
            throw new ApplicationServiceException("An account for this email address already exists.");
        user.ChangeEmail(email);
        return user;
    }

    private async Task<User> ChangeName(string name, User user)
    {
        user.Rename(name);
        return user;
    }

    private async Task<User> ChangeDoB(DateOnly dob, User user)
    {
        user.ChangeDateOfBirth(dob, _clock.UtcNow);
        return user;
    }

    private async Task<User> ChangePassword(string plainPassword, User user)
    {
        string hashedPassword = _hasher.Hash(plainPassword);
        user.ChangePassword(hashedPassword);
        return user;
    }

    private async Task<User> ChangeRole(int role, User user)
    {
        //Normalmente validaríamos la autorización del usuario que hace el cambio de rol, pero aún no hemos implementado jwt, así que queda para después.
        if (!Enum.IsDefined(typeof(UserRole), role))
            throw new ApplicationServiceException("Invalid role specified.");
        user.SetRole((UserRole)role);
        return user;
    }
    
    public async Task DeleteUser(Guid id, CancellationToken ct)
    {
        User? userToDelete = await _users.GetById((UserId)id, ct);
        if (userToDelete is null)
            throw new ApplicationServiceException("Requested user not found");
        _users.Delete(userToDelete, ct);
    }
}