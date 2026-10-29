

using MyApp.Application.Abstractions;
using MyApp.Application.Interfaces;
using MyApp.Application.Models;
using MyApp.Domain.Entities;
using MyApp.Domain.Exceptions;
using MyApp.Domain.ValueObjects;

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
        if (user is null) throw new ApplicationServiceException("Requested user not found");
        // Verificamos que la contraseña sea correcta
        bool flag = _hasher.Verify(loginData.PlainPassword, user.Password.ToString());
        // Si la contraseña es correcta, devolvemos el token
        if (flag)
            return _jwt.GenerateToken(user.Id, user.Role);
        throw new ApplicationServiceException("Password did not match.");
    }

    public async Task<UserDTO> ValidateToken(string token, CancellationToken ct)
    {
        var val = _jwt.ValidateToken(token);
        if (val is null || val.FindFirst("user") is null) throw new ApplicationServiceException("Token has no user.");
        Guid user = Guid.Parse(val.FindFirst("user")?.Value ?? $"{Guid.Empty}");
        if (user == Guid.Empty) throw new ApplicationServiceException("Token user is invalid.");
        User? fullUser = await _repo.GetById((UserId)user, ct);
        if (fullUser is null) throw new ApplicationServiceException("Token user not found.");;
        return UserDTO.Create(fullUser, _clock.UtcNow);
    } 

}