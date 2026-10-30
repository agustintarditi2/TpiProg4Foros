using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MyApp.Application.Abstractions;
using MyApp.Domain.Entities;

namespace MyApp.Infrastructure.Security;

public sealed class JwtTokenService : ITokenService
{
    private readonly JwtSettings _settings;
    private readonly IClock _clock;

    public JwtTokenService(IOptions<JwtSettings> options, IClock clock)
    {
        _settings = options.Value;
        _clock = clock;

        if (Encoding.UTF8.GetByteCount(_settings.Key) < 32)
            throw new InvalidOperationException(
                "Jwt:Key is missing or shorter than 32 bytes. Set it with 'dotnet user-secrets'.");
    }

    public (string Token, DateTime ExpiresAtUtc) GenerateToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.Value.ToString()),
            new Claim(ClaimTypes.Email, user.Email.Value),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = _clock.UtcNow.UtcDateTime.AddMinutes(_settings.ExpiresMinutes);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}