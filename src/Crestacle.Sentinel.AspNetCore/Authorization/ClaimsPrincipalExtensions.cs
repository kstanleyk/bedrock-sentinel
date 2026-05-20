using System.Security.Claims;

namespace Crestacle.Sentinel.AspNetCore.Authorization;

public static class ClaimsPrincipalExtensions
{
    /// <summary>Returns the subject (identity ID) from the NameIdentifier claim.</summary>
    public static string? GetSubject(this ClaimsPrincipal user)
        => user.FindFirstValue(ClaimTypes.NameIdentifier);
}
