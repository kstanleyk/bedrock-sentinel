using System.Security.Claims;
using Crestacle.Bedrock.AspNetCore.Authorization;
using Crestacle.Bedrock.AspNetCore.Models;
using Crestacle.Bedrock.Core.DTOs;
using Crestacle.Bedrock.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crestacle.Bedrock.AspNetCore.Controllers;

[ApiController]
[Route("auth")]
public sealed class BedrockPasswordController : ControllerBase
{
    private readonly ICredentialService _credentials;

    public BedrockPasswordController(ICredentialService credentials)
        => _credentials = credentials;

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult<BedrockResponse>> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken ct)
    {
        await _credentials.RequestPasswordResetAsync(request.Email, ct);
        return Ok(BedrockResponse.Ok());
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult<BedrockResponse>> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        await _credentials.ResetPasswordAsync(request.Token, request.NewPassword, ip, ct);
        return Ok(BedrockResponse.Ok());
    }

    [HttpPost("change-password")]
    [Authorize(Policy = BedrockPolicyNames.Default)]
    public async Task<ActionResult<BedrockResponse>> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        await _credentials.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword, ip, ct);
        return Ok(BedrockResponse.Ok());
    }
}
