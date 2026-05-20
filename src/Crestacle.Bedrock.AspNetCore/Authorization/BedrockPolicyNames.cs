namespace Crestacle.Bedrock.AspNetCore.Authorization;

/// <summary>Well-known authorization policy names registered by <c>AddBedrockAspNetCore</c>.</summary>
public static class BedrockPolicyNames
{
    /// <summary>
    /// Requires an authenticated user whose access token is NOT enrollment-scoped.
    /// Apply to all standard business endpoints.
    /// </summary>
    public const string Default = "BedrockDefault";

    /// <summary>
    /// Requires a token with <c>token_type == "enrollment"</c>.
    /// Apply to the MFA enrollment endpoints (setup-totp, confirm-totp, setup-otp).
    /// </summary>
    public const string MfaEnrollment = "BedrockMfaEnrollment";

    /// <summary>
    /// Requires an authenticated user with a <c>bedrock_admin = "true"</c> claim.
    /// Grant this claim via <c>IBedrockClaimsEnricher</c> for trusted admin principals.
    /// </summary>
    public const string Admin = "BedrockAdmin";
}
