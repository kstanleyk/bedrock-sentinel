using Crestacle.Sentinel.AspNetCore.Authorization;
using Crestacle.Sentinel.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Crestacle.Sentinel.AspNetCore.Controllers;

[ApiController]
[Route("api/auth/permissions")]
[Authorize]
public sealed class PermissionsController(IUserPermissionRepository userPermissionRepository) : ControllerBase
{
    /// <summary>Returns all permissions held by the currently authenticated user.</summary>
    [HttpGet("me")]
    [ProducesResponseType<IEnumerable<string>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyPermissions(CancellationToken cancellationToken)
    {
        var subject = User.GetSubject();

        if (string.IsNullOrWhiteSpace(subject))
            return Unauthorized();

        var permissions = await userPermissionRepository.GetPermissionsForUserAsync(subject, cancellationToken);
        return Ok(permissions);
    }
}
