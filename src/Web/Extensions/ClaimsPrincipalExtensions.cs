using System.Security.Claims;
using MyApp.Application.Exceptions;

namespace MyApp.Web.Extensions;
// permite obtener y validar el ID del usuario
public static class ClaimsPrincipalExtensions
{
    public static Guid GetAuthenticatedUserId(this ClaimsPrincipal user)
    {
        string? userClaim = user.FindFirst("user")?.Value;

        if (!Guid.TryParse(userClaim, out Guid userId) || userId == Guid.Empty)
            throw new AuthenticationFailedException("Invalid user identity.");

        return userId;
    }
}