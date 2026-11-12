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
            throw new DomainException("Email is already registered.");

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
            throw new NotFoundException("Requested user not found");
        return UserDTO.Create(foundUser, _clock.UtcNow);
    }

    
//=========================================================================



    public async Task<List<UserDTO>> Get(CancellationToken ct)
    {
        List<User>? userList = await _users.Get(ct);
        if (userList.Count < 1)
            throw new NotFoundException("No users found");
        return UserDTO.CreateList(userList, _clock.UtcNow);
    }

    
//=========================================================================



    public async Task UpdateUser(PatchUserDTO user,Guid authenticatedUserId, CancellationToken ct)
    {
        // se busca al usuario por el ID obtenido del JWT
        User? changedUser = await _users.GetById(
            (UserId)authenticatedUserId, ct);

        if (changedUser is null)
            throw new NotFoundException("Requested user not found");

        if (user.Email is null &&
            user.Name is null &&
            user.PlainPassword is null)
            return;

        if (user.Email is string email)
            changedUser = await ChangeEmail(email, changedUser);

        if (user.Name is string name)
            changedUser = await ChangeName(name, changedUser);

        if (user.PlainPassword is string plainPassword)
            changedUser = await ChangePassword(plainPassword, changedUser);

        await _users.Update(changedUser, ct);
    }


// Métodos que se llaman para modificar al usuario que después devuelve UpdateUser()

    private async Task<User> ChangeEmail(string email, User user)
    {
        if (user.Email.ToString()==email)
            return user;
        if (await _users.EmailExists(EmailAddress.Create(email)))
            throw new DomainException("An account for this email address already exists.");
        user.ChangeEmail(email);
        return user;
    }

    private async Task<User> ChangeName(string name, User user)
    {
        user.Rename(name);
        return user;
    }


    private async Task<User> ChangePassword(string plainPassword, User user)
    {
        string hashedPassword = _hasher.Hash(plainPassword);
        user.ChangePassword(hashedPassword);
        return user;
    }

    public async Task ChangeUserRole(Guid targetUserId, int role, Guid authenticatedUserId, CancellationToken ct)
{
    // Verifica que quien realiza el cambio sea SysAdmin
    User? admin = await _users.GetById(
        (UserId)authenticatedUserId, ct);

    if (admin is null || admin.Role != UserRole.SysAdmin)
        throw new UnauthorizedAccessException(
            "Only SysAdmin can change user roles.");

    // Nadie puede modificar su propio rol
    if (targetUserId == authenticatedUserId)
        throw new UnauthorizedAccessException(
            "You cannot change your own role.");

    // Valida que el rol exista
    if (!Enum.IsDefined(typeof(UserRole), role))
        throw new DomainException("Invalid role specified.");

    // Busca al usuario que recibirá el nuevo rol
    User? targetUser = await _users.GetById(
        (UserId)targetUserId, ct);

    if (targetUser is null)
        throw new NotFoundException("Requested user not found");

    targetUser.SetRole((UserRole)role);

    await _users.Update(targetUser, ct);
}
    
    public async Task DeleteOwnAccount(
    Guid authenticatedUserId,
    CancellationToken ct)
    {
        // busca la cuenta del usuario autenticado.
        User? userToDelete = await _users.GetById(
            (UserId)authenticatedUserId, ct);

        if (userToDelete is null)
            throw new NotFoundException("Requested user not found");

        /// Genera una contraseña aleatoria que no se guardara
        string disabledHash = _hasher.Hash(
            Guid.NewGuid().ToString("N"));

        // Desactiva la cuenta sin eliminar sus registros
        userToDelete.DeactivateAccount(disabledHash);

        await _users.Update(userToDelete, ct);
        }
}