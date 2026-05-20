using Crestacle.Bedrock.Core.DTOs;
using Crestacle.Bedrock.Core.Exceptions;

namespace Crestacle.Bedrock.Core.Interfaces.Services;

/// <summary>
/// Orchestrates step-up re-authentication: issues a short-lived OTP or TOTP challenge
/// and verifies the response, returning a single-use step-up JWT.
/// </summary>
public interface IStepUpService
{
    /// <summary>
    /// Creates a step-up challenge for the user using their current MFA method,
    /// sends the OTP if required, and returns the challenge descriptor.
    /// </summary>
    /// <param name="userId">The authenticated user requesting a step-up challenge.</param>
    /// <param name="ip">The client IP address for audit logging.</param>
    /// <param name="userAgent">The client User-Agent header for audit logging.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="StepUpChallengeResult"/> containing the challenge ID and the required verification method.</returns>
    Task<StepUpChallengeResult> InitiateAsync(
        Guid userId,
        string ip,
        string userAgent,
        CancellationToken ct = default);

    /// <summary>
    /// Verifies the step-up code against the challenge identified by <paramref name="challengeId"/>,
    /// marks the challenge used, and returns a signed step-up JWT.
    /// </summary>
    /// <param name="userId">The user who initiated the challenge; must match the challenge record.</param>
    /// <param name="challengeId">The challenge identifier returned by <see cref="InitiateAsync"/>.</param>
    /// <param name="code">The TOTP or OTP code supplied by the user.</param>
    /// <param name="ip">The client IP address for audit logging.</param>
    /// <param name="userAgent">The client User-Agent header for audit logging.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A signed step-up JWT valid for a short duration.</returns>
    /// <exception cref="BedrockValidationException">Thrown when the challenge does not exist, is already used, is expired, or the code is incorrect.</exception>
    Task<string> VerifyAsync(
        Guid userId,
        Guid challengeId,
        string code,
        string ip,
        string userAgent,
        CancellationToken ct = default);
}
