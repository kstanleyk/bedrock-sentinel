using System.Security.Claims;
using Crestacle.Bedrock.AspNetCore.Authorization;
using Crestacle.Bedrock.AspNetCore.Models;
using Crestacle.Bedrock.Core.DTOs;
using Crestacle.Bedrock.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crestacle.Bedrock.AspNetCore.Controllers;

[ApiController]
[Route("step-up")]
[Authorize(Policy = BedrockPolicyNames.Default)]
public sealed class BedrockStepUpController : ControllerBase
{
    private readonly IStepUpService _stepUp;

    public BedrockStepUpController(IStepUpService stepUp) => _stepUp = stepUp;

    /// <summary>
    /// Initiates a step-up challenge using the authenticated user's current MFA method.
    /// Returns the challenge ID and method; OTP is dispatched if applicable.
    /// </summary>
    [HttpPost("initiate")]
    public async Task<ActionResult<BedrockResponse<StepUpInitiateResponse>>> Initiate(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = Request.Headers.UserAgent.ToString();
        if (string.IsNullOrWhiteSpace(userAgent)) userAgent = "unknown";

        var result = await _stepUp.InitiateAsync(userId, ip, userAgent, ct);
        return Ok(BedrockResponse<StepUpInitiateResponse>.Ok(
            new StepUpInitiateResponse(result.ChallengeId, result.Method)));
    }

    /// <summary>
    /// Verifies the step-up code and returns a single-use step-up JWT in
    /// <see cref="StepUpVerifyResponse.StepUpToken"/>. Present this token in the
    /// <c>X-Step-Up-Token</c> header when calling endpoints decorated with
    /// <see cref="RequiresStepUpAttribute"/>.
    /// </summary>
    [HttpPost("verify")]
    public async Task<ActionResult<BedrockResponse<StepUpVerifyResponse>>> Verify(
        [FromBody] VerifyStepUpRequest request,
        CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = Request.Headers.UserAgent.ToString();
        if (string.IsNullOrWhiteSpace(userAgent)) userAgent = "unknown";

        var token = await _stepUp.VerifyAsync(userId, request.ChallengeId, request.Code, ip, userAgent, ct);
        return Ok(BedrockResponse<StepUpVerifyResponse>.Ok(new StepUpVerifyResponse(token)));
    }
}
