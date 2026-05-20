using Microsoft.AspNetCore.Authorization;

namespace Crestacle.Sentinel.AspNetCore.Authorization;

internal sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
