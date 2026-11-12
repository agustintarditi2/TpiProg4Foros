using Microsoft.AspNetCore.Authorization;

public sealed record UpdateRequest(Guid UserId);

public sealed class OwnerOrAdminRequirement : IAuthorizationRequirement { }

public sealed class OwnerOrAdminHandler
    : AuthorizationHandler<OwnerOrAdminRequirement, UpdateRequest>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OwnerOrAdminRequirement requirement,
        UpdateRequest request)
    {
        Guid.TryParse(context.User.FindFirst("user")?.Value, out Guid userId);
        if (context.User.IsInRole("Admin") || userId == request.UserId)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
