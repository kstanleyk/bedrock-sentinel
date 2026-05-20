namespace Crestacle.Bedrock.Core.Interfaces.Services;

/// <summary>
/// Generates and validates all JWT and opaque tokens used by Bedrock.
/// </summary>
public interface ITokenService
{
    /// <summary>Issues a signed access JWT containing standard and role claims.</summary>
    /// <param name="userId">The subject user identifier, embedded as <c>sub</c> and <c>user_id</c> claims.</param>
    /// <param name="email">The user's email address, embedded as an <c>email</c> claim.</param>
    /// <param name="roles">Role names to embed as <c>role</c> claims.</param>
    /// <param name="tenantId">Optional tenant identifier embedded as a <c>tenant_id</c> claim.</param>
    /// <param name="extraClaims">Optional additional claims to embed verbatim.</param>
    /// <returns>A signed JWT string.</returns>
    string GenerateAccessToken(
        Guid userId,
        string email,
        IEnumerable<string> roles,
        string? tenantId = null,
        IDictionary<string, string>? extraClaims = null);

    /// <summary>Generates a cryptographically random 64-byte opaque refresh token (Base64 encoded).</summary>
    /// <returns>A Base64-encoded 64-byte random token string.</returns>
    string GenerateRefreshToken();

    /// <summary>Returns the SHA-256 hex digest of a raw token string.</summary>
    /// <param name="rawToken">The plaintext token to hash.</param>
    /// <returns>A lowercase hexadecimal SHA-256 digest.</returns>
    string HashToken(string rawToken);

    /// <summary>Issues a short-lived challenge JWT embedding the challenge ID as a claim.</summary>
    /// <param name="challengeId">The MFA challenge identifier to embed.</param>
    /// <param name="userId">The user the challenge belongs to.</param>
    /// <returns>A signed short-lived challenge JWT string.</returns>
    string GenerateChallengeToken(Guid challengeId, Guid userId);

    /// <summary>Issues a short-lived step-up JWT embedding the challenge ID.</summary>
    /// <param name="challengeId">The step-up challenge identifier to embed.</param>
    /// <param name="userId">The user the step-up challenge belongs to.</param>
    /// <returns>A signed short-lived step-up JWT string.</returns>
    string GenerateStepUpToken(Guid challengeId, Guid userId);

    /// <summary>Issues a scope-limited enrollment JWT; contains no role claims.</summary>
    /// <param name="userId">The user beginning MFA enrollment.</param>
    /// <returns>A signed enrollment JWT valid for 15 minutes.</returns>
    string GenerateEnrollmentToken(Guid userId);

    /// <summary>
    /// Validates a challenge token; returns the challenge ID when valid,
    /// or <c>null</c> when the token is expired, has the wrong type, or has an invalid signature.
    /// </summary>
    /// <param name="token">The challenge JWT to validate.</param>
    /// <returns>The embedded challenge ID, or <c>null</c> on any validation failure.</returns>
    Guid? ValidateChallengeToken(string token);

    /// <summary>
    /// Validates a step-up token; returns the challenge ID when valid,
    /// or <c>null</c> when the token is expired, has the wrong type, or has an invalid signature.
    /// </summary>
    /// <param name="token">The step-up JWT to validate.</param>
    /// <returns>The embedded challenge ID, or <c>null</c> on any validation failure.</returns>
    Guid? ValidateStepUpToken(string token);

    /// <summary>
    /// Validates a step-up token and confirms the embedded user ID matches <paramref name="expectedUserId"/>.
    /// Returns <c>(true, challengeId)</c> on success; <c>(false, null)</c> on any failure.
    /// </summary>
    /// <param name="token">The step-up JWT to validate.</param>
    /// <param name="expectedUserId">The user ID that must match the token's <c>user_id</c> claim.</param>
    /// <returns>A tuple of validation success and the embedded challenge ID (or <c>null</c> on failure).</returns>
    (bool isValid, Guid? challengeId) ValidateAndExtractStepUp(string token, Guid expectedUserId);

    /// <summary>Reads the <c>jti</c> claim from a JWT without re-validating the signature.</summary>
    /// <param name="jwtToken">The raw JWT string to parse.</param>
    /// <returns>The <c>jti</c> claim value.</returns>
    string ExtractJti(string jwtToken);
}
