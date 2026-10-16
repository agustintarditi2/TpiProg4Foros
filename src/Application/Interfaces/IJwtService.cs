namespace MyApp.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(Guid userId, string role, TimeSpan? expires = null);

}