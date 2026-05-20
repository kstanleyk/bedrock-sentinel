using System.Security.Claims;
using Crestacle.Bedrock.AspNetCore.Authorization;
using Crestacle.Bedrock.AspNetCore.Middleware;
using Crestacle.Bedrock.AspNetCore.Models;
using Crestacle.Bedrock.Core.DTOs;
using Crestacle.Bedrock.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crestacle.Bedrock.AspNetCore.Controllers;

[ApiController]
[Route("2fa")]
public sealed class BedrockMfaController : ControllerBase
{
    private readonly ICredentialService _credentials;

    public BedrockMfaController(ICredentialService credentials) => _credentials = credentials;

    [HttpPost("setup-totp")]
    [EnrollmentEndpoint]
    [Authorize(Policy = BedrockPolicyNames.MfaEnrollment)]
    public async Task<ActionResult<BedrockResponse<TotpSetupResponse>>> SetupTotp(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _credentials.SetupTotpAsync(userId, ct);
        return Ok(BedrockResponse<TotpSetupResponse>.Ok(new TotpSetupResponse(result.QrUri)));
    }

    [HttpPost("confirm-totp")]
    [EnrollmentEndpoint]
    [Authorize(Policy = BedrockPolicyNames.MfaEnrollment)]
    public async Task<ActionResult<BedrockResponse<RecoveryCodesResponse>>> ConfirmTotp(
        [FromBody] ConfirmTotpRequest request,
        CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _credentials.ConfirmTotpAsync(userId, request.Code, ct);
        return Ok(BedrockResponse<RecoveryCodesResponse>.Ok(new RecoveryCodesResponse(result.Codes)));
    }

    [HttpPost("setup-otp")]
    [EnrollmentEndpoint]
    [Authorize(Policy = BedrockPolicyNames.MfaEnrollment)]
    public async Task<ActionResult<BedrockResponse<RecoveryCodesResponse>>> SetupOtp(
        [FromBody] SetupOtpRequest request,
        CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _credentials.SetupOtpAsync(userId, request.Method, ct);
        return Ok(BedrockResponse<RecoveryCodesResponse>.Ok(new RecoveryCodesResponse(result.Codes)));
    }

    [HttpGet("recovery-codes")]
    [Authorize(Policy = BedrockPolicyNames.Default)]
    [RequiresStepUp]
    public async Task<ActionResult<BedrockResponse<RemainingRecoveryCodesResponse>>> GetRemainingRecoveryCodes(
        CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var count = await _credentials.GetRemainingRecoveryCodeCountAsync(userId, ct);
        return Ok(BedrockResponse<RemainingRecoveryCodesResponse>.Ok(
            new RemainingRecoveryCodesResponse(count)));
    }

    [HttpPost("recovery-codes/regenerate")]
    [Authorize(Policy = BedrockPolicyNames.Default)]
    [RequiresStepUp]
    public async Task<ActionResult<BedrockResponse<RecoveryCodesResponse>>> RegenerateRecoveryCodes(
        CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _credentials.RegenerateRecoveryCodesAsync(userId, ct);
        return Ok(BedrockResponse<RecoveryCodesResponse>.Ok(new RecoveryCodesResponse(result.Codes)));
    }

    [HttpPost("disable")]
    [Authorize(Policy = BedrockPolicyNames.Default)]
    [RequiresStepUp]
    public async Task<ActionResult<BedrockResponse>> DisableMfa(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _credentials.DisableMfaAsync(userId, ct);
        return Ok(BedrockResponse.Ok());
    }
}
