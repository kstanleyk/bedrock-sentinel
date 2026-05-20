using Crestacle.Sentinel.Core.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Crestacle.Sentinel.AspNetCore.Authorization;

/// <summary>
/// Requires the authenticated user to hold the specified feature/action permission.
/// Usage: [MustHavePermission(AppFeature.Invoice, AppAction.Read)]
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class MustHavePermissionAttribute : AuthorizeAttribute
{
    public MustHavePermissionAttribute(string feature, string action)
        => Policy = AppPermission.NameFor(feature, action);
}
