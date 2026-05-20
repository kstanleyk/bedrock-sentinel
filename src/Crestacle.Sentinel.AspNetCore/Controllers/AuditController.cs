using Crestacle.Sentinel.AspNetCore.Authorization;
using Crestacle.Sentinel.Core.Authorization;
using Crestacle.Sentinel.Core.DTOs;
using Crestacle.Sentinel.Core.Enums;
using Crestacle.Sentinel.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Crestacle.Sentinel.AspNetCore.Controllers;

[ApiController]
[Route("api/auth/audit")]
public sealed class AuditController(IAuditRepository auditRepository) : ControllerBase
{
    /// <summary>
    /// Returns the immutable audit log, newest entries first.
    /// All query parameters are optional and can be combined.
    /// </summary>
    [HttpGet]
    [MustHavePermission(SentinelFeature.Audit, AppAction.Read)]
    [ProducesResponseType<PagedResult<AuditEntryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAuditLog(
        [FromQuery] string?      actorIdentityId = null,
        [FromQuery] AuditAction? action          = null,
        [FromQuery] DateTime?    from            = null,
        [FromQuery] DateTime?    to              = null,
        [FromQuery] int          page            = 1,
        [FromQuery] int          pageSize        = 50,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)        return BadRequest(new { error = "page must be >= 1." });
        if (pageSize is < 1 or > 200) return BadRequest(new { error = "pageSize must be between 1 and 200." });
        if (from.HasValue && to.HasValue && from > to)
            return BadRequest(new { error = "'from' must be earlier than 'to'." });

        var result = await auditRepository.GetAsync(
            actorIdentityId, action, from, to, page, pageSize, cancellationToken);

        return Ok(result);
    }
}
