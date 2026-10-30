

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
    public AuthServices(
        IUserRepository userRepository, 
        IPasswordHasher hasher,
        IJwtServices jwt,
        IClock clock
        )
    {
        _repo = userRepository;
        _hasher = hasher;
        _jwt = jwt;
        _clock = clock;
    }
    
    public async Task<string> Authenticate(LoginUserDTO loginData, CancellationToken ct)
    {
        // Buscamos el usuario
        User? user = await _repo.GetByEmail(EmailAddress.Create(loginData.Email), ct);
        // Tiramos excepción si no existe
        if (user is null) throw new AuthenticationFailedException("Invalid credentials");
        // Verificamos que la contraseña sea correcta
        bool flag = _hasher.Verify(loginData.PlainPassword, user.Password.ToString());
        // Si la contraseña es correcta, devolvemos el token
        if (flag)
            return _jwt.GenerateToken(user.Id, user.Role);
        throw new AuthenticationFailedException("Invalid credentials");
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

        if (fullUser is null) 
            throw new AuthenticationFailedException("Token user not found.");;

        return UserDTO.Create(fullUser, _clock.UtcNow);
    } 

}