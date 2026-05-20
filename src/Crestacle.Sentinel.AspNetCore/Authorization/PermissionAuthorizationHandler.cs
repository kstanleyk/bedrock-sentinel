using Crestacle.Sentinel.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace Crestacle.Sentinel.AspNetCore.Authorization;

internal sealed class PermissionAuthorizationHandler(IUserPermissionRepository userPermissionRepository)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var subject = context.User.GetSubject();

        if (string.IsNullOrWhiteSpace(subject))
        {
            context.Fail();
            return;
        }

        var userPermissions = await userPermissionRepository.GetPermissionsForUserAsync(subject);

        if (userPermissions.Contains(requirement.Permission))
            context.Succeed(requirement);
    }
}
