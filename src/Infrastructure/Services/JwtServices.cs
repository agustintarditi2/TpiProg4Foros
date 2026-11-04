using MyApp.Application.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MyApp.Domain.Enums;
using MyApp.Domain.ValueObjects;

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
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var principal = handler.ValidateToken(token, parameters, out SecurityToken validatedToken);

                return principal;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Token inválido: {ex.Message}");
                throw new Exception("Token inválido o expirado.", ex);
            }

        }
}