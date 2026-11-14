

using MyApp.Application.Abstractions;
using MyApp.Application.Interfaces;
using MyApp.Application.Models;
using MyApp.Domain.Entities;
using MyApp.Domain.Exceptions;
using MyApp.Domain.ValueObjects;
using MyApp.Application.Exceptions;

namespace MyApp.Application.Services;

public class AuthServices
{
    private readonly IUserRepository _repo;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtServices _jwt;
    private readonly IClock _clock;
    private readonly BanServices _bans;
    public AuthServices(
        IUserRepository userRepository, 
        IPasswordHasher hasher,
        BanServices banServices,
        IJwtServices jwt,
        IClock clock
        )
    {
        _repo = userRepository;
        _hasher = hasher;
         _bans = banServices;
        _jwt = jwt;
        _clock = clock;
    }
    
    public async Task<string> Authenticate(LoginUserDTO loginData, CancellationToken ct)
    {
        // Buscamos el usuario
        User? user = await _repo.GetByEmail(EmailAddress.Create(loginData.Email), ct);
        // Tiramos excepción si no existe
        if (user is null || !user.IsActive) throw new AuthenticationFailedException("Invalid credentials");
        // Verificamos que la contraseña sea correcta
        bool flag = _hasher.Verify(loginData.PlainPassword, user.Password.ToString());
        // Si la contraseña es correcta, devolvemos el token
        if (!flag)
            throw new AuthenticationFailedException("Invalid credentials");
        // Verifica que el usuario no tenga un baneo vigente.
        await EnsureNotBannedForAuth(user.Id.Value, ct);

        return _jwt.GenerateToken(user.Id, user.Role);
    }

    public async Task<UserDTO> ValidateToken(string token, CancellationToken ct)
    {   

        var val = _jwt.ValidateToken(token);
         // Busca el identificador del usuario dentro de los claims del JWT
        string? userClaim = val?.FindFirst("user")?.Value;

        if (userClaim is null)
            throw new AuthenticationFailedException("Token has no user.");
        // Comprueba que el identificador sea un guid valido
        if (!Guid.TryParse(userClaim, out Guid user) || user == Guid.Empty)
            throw new AuthenticationFailedException("Token user is invalid.");
        // Busca al usuario en la base de datos
        User? fullUser = await _repo.GetById((UserId)user, ct);

        if (fullUser is null || !fullUser.IsActive) 
            throw new AuthenticationFailedException("Token user not found or account is inactive.");
        
        // comprueba si el usuario fue baneado despues de conseguir el jwt
        await EnsureNotBannedForAuth(fullUser.Id.Value, ct);

        return UserDTO.Create(fullUser, _clock.UtcNow);
    } 

    private async Task EnsureNotBannedForAuth(
    Guid userId,
    CancellationToken ct)
    {
        try
        {
            await _bans.EnsureNotBanned(userId, ct);
        }
        catch (UnauthorizedAccessException)
        {
            throw new AuthenticationFailedException(
                "Your account is temporarily banned.");
        }
    }
}