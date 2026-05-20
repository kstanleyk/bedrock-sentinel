namespace Crestacle.Bedrock.Core.DTOs;

/// <summary>
/// Unified result of a first-factor login attempt. Exactly one of the outcome fields will
/// be populated depending on the user's MFA state and policy configuration.
/// </summary>
public sealed record LoginResult
{
    /// <summary>True when MFA verification is required before full tokens can be issued.</summary>
    public bool RequiresMfa { get; init; }

    /// <summary>
    /// True when the user must enroll an MFA method (mandatory MFA policy, grace period expired).
    /// </summary>
    public bool RequiresEnrollment { get; init; }

    /// <summary>Full token pair; populated when authentication is complete with no further steps.</summary>
    public TokenPair? Tokens { get; init; }

    /// <summary>MFA challenge details; populated when <see cref="RequiresMfa"/> is true.</summary>
    public MfaChallengeResult? Challenge { get; init; }

    /// <summary>
    /// Scope-limited enrollment token; populated when <see cref="RequiresEnrollment"/> is true.
    /// Only valid on the MFA enrollment endpoints.
    /// </summary>
    public string? EnrollmentToken { get; init; }

    /// <summary>
    /// UTC end of the user's MFA grace period; present when the user is in the grace period
    /// and full tokens have been issued. Used by the client UI to prompt for MFA setup.
    /// </summary>
    public DateTime? MfaGracePeriodEndsAt { get; init; }
}
