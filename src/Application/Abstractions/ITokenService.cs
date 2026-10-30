using MyApp.Domain.Entities;

namespace MyApp.Application.Abstractions;

public interface ITokenService
{
    (string Token, DateTime ExpiresAtUtc) GenerateToken(User user);
}