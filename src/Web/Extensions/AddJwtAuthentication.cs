using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MyApp.Application.Interfaces;
using MyApp.Domain.ValueObjects;
using MyApp.Application.Services;

namespace MyApp.Web.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var secretKey = configuration["Authentication:SecretForKey"];

        if (string.IsNullOrWhiteSpace(secretKey))
        {
            throw new InvalidOperationException(
                "Authentication:SecretForKey no está configurada."
            );
        }

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = configuration["Authentication:Issuer"],
                        ValidateAudience = true,
                        ValidAudience = configuration["Authentication:Audience"],
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(secretKey)
                        ),
                        ValidateLifetime = true,
                        RoleClaimType = System.Security.Claims.ClaimTypes.Role
                    };
                     options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        // Obtiene el ID del usuario desde el JWT
                        string? userClaim = context.Principal?
                            .FindFirst("user")?.Value;

                        if (!Guid.TryParse(userClaim, out Guid userId) ||
                            userId == Guid.Empty)
                        {
                            context.Fail("Invalid user identity.");
                            return;
                        }

                        // Busca al usuario en la base de datos.
                        var repository = context.HttpContext.RequestServices
                            .GetRequiredService<IUserRepository>();

                        var user = await repository.GetById(
                            (UserId)userId,
                            context.HttpContext.RequestAborted);

                        // Rechaza cuentas inexistentes o desactivadas
                        if (user is null || !user.IsActive)
                        {
                            context.Fail("User account is inactive.");
                            return;
                        }

                        // Comprueba que el rol siga siendo válido
                        if (!context.Principal!.IsInRole(user.Role.ToString()))
                        {
                            context.Fail("User role has changed.");
                            return;
                        }
                        // Obtiene el servicio que verifica los bans
                        var banServices = context.HttpContext.RequestServices
                            .GetRequiredService<BanServices>();

                        try
                        {
                            await banServices.EnsureNotBanned(
                                user.Id.Value,
                                context.HttpContext.RequestAborted);
                        }
                        catch (UnauthorizedAccessException)
                        {
                            context.Fail("User account is banned.");
                            return;
                        }
                    }
                };
            });

        services.AddAuthorization();
        
        return services;
    }
}
