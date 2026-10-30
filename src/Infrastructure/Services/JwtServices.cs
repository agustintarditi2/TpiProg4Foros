using MyApp.Application.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MyApp.Domain.Enums;
using MyApp.Domain.ValueObjects;
using MyApp.Application.Exceptions;

namespace MyApp.Infrastructure.Services;

public class JwtServices : IJwtServices
{
    private readonly IConfiguration _config;

    public JwtServices(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(UserId userId, UserRole role)
    {
        var claims = new List<Claim>
        {
            new Claim("role", role.ToString()),
            new Claim("user", userId.Value.ToString())
        };
        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_config["Authentication:SecretForKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(issuer: _config["Authentication:Issuer"],
            audience: _config["Authentication:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.Add(TimeSpan.FromHours(1)),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public ClaimsPrincipal ValidateToken(string token)
        {
            var key = Encoding.ASCII.GetBytes(_config["Authentication:SecretForKey"]!);
            var parameters = new TokenValidationParameters
            {   // Verifica que el token tenga una firma válida
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                // Verifica que el token haya sido emitido por el usuario autorizado
                ValidateIssuer = true,
                ValidIssuer = _config["Authentication:Issuer"],
                // Verifica que el token este dirigidio a la api
                ValidateAudience = true,
                ValidAudience = _config["Authentication:Audience"],
                // Verifica que el token no este vencido
                ValidateLifetime = true,
                 // No permite tolerancia adicional en las fechas de validez
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var principal = handler.ValidateToken(token, parameters, out SecurityToken validatedToken);

                return principal;
            }
            catch (SecurityTokenException)
            {
                throw new AuthenticationFailedException("Invalid or expired token.");
            }
            catch (ArgumentException)
            {
                throw new AuthenticationFailedException("Invalid or expired token.");
            }

        }
}