using System.Security.Claims;
using MyApp.Domain.Enums;
using MyApp.Domain.ValueObjects;

namespace MyApp.Application.Interfaces;

public interface IJwtServices
{
    string GenerateToken(UserId userId, UserRole role);
    ClaimsPrincipal ValidateToken(string token);

}