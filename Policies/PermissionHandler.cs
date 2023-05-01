using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace DemoIdentity.Policies;

// class xử lý policy nếu có nhiều requirement
public class PermissionHandler : IAuthorizationHandler
{
    private class ReadPermission : IAuthorizationRequirement
    {
    }

    private class EditPermission : IAuthorizationRequirement
    {
    }

    private class DeletePermission : IAuthorizationRequirement
    {
    }

    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        var pendingRequirements = context.PendingRequirements.ToList();

        foreach (var requirement in pendingRequirements)
        {
            if (requirement is ReadPermission)
            {
                if (IsOwner(context.User, context.Resource)
                    || IsSponsor(context.User, context.Resource))
                {
                    context.Succeed(requirement);
                }
            }
            else if (requirement is EditPermission || requirement is DeletePermission)
            {
                if (IsOwner(context.User, context.Resource))
                {
                    context.Succeed(requirement);
                }
            }
        }

        return Task.CompletedTask;
    }

    private static bool IsOwner(ClaimsPrincipal user, object? resource)
    {
        // Code omitted for brevity
        return true;
    }

    private static bool IsSponsor(ClaimsPrincipal user, object? resource)
    {
        // Code omitted for brevity
        return true;
    }
}