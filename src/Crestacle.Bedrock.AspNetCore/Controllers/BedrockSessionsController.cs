using System.Security.Claims;
using Crestacle.Bedrock.AspNetCore.Authorization;
using Crestacle.Bedrock.AspNetCore.Models;
using Crestacle.Bedrock.Core.DTOs;
using Crestacle.Bedrock.Core.Entities;
using Crestacle.Bedrock.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crestacle.Bedrock.AspNetCore.Controllers;

[ApiController]
[Route("sessions")]
[Authorize(Policy = BedrockPolicyNames.Default)]
public sealed class BedrockSessionsController : ControllerBase
{
    private readonly ISessionService _sessions;

    public BedrockSessionsController(ISessionService sessions) => _sessions = sessions;

    [HttpGet]
    public async Task<ActionResult<BedrockResponse<IReadOnlyList<SessionResponse>>>> GetSessions(
        CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var sessions = await _sessions.GetActiveSessionsAsync(userId, ct);
        var response = sessions.Select(Map).ToList();
        return Ok(BedrockResponse<IReadOnlyList<SessionResponse>>.Ok(response));
    }

    [HttpDelete("{sessionId:guid}")]
    public async Task<ActionResult<BedrockResponse>> RevokeSession(
        Guid sessionId,
        CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        await _sessions.RevokeSessionAsync(sessionId, userId, ip, ct);
        return Ok(BedrockResponse.Ok());
    }

    [HttpDelete]
    public async Task<ActionResult<BedrockResponse>> RevokeAllSessions(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        await _sessions.RevokeAllSessionsAsync(userId, ip, ct);
        return Ok(BedrockResponse.Ok());
    }

    private static SessionResponse Map(Session s) => new(
        s.Id,
        s.DeviceFingerprint,
        s.IpAddress,
        s.UserAgent,
        s.CreatedAt,
        s.LastActivityAt);
}
