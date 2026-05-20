namespace Crestacle.Bedrock.Core.DTOs;

/// <summary>
/// Result of first-factor credential verification. The caller uses the populated fields
/// to decide whether to issue tokens, initiate an MFA challenge, require enrollment, or
/// return an error response.
/// </summary>
public sealed record FirstFactorResult
{
    public bool Succeeded { get; init; }
    public Guid UserId { get; init; }
    public bool MfaEnabled { get; init; }
    public bool IsLockedOut { get; init; }
    public DateTime? LockoutEnd { get; init; }

    /// <summary>Populated when MFA is required; the client must exchange this for a full token pair.</summary>
    public MfaChallengeResult? Challenge { get; init; }

    /// <summary>True when mandatory MFA is configured and the user's grace period has expired.</summary>
    public bool RequiresEnrollment { get; init; }

    /// <summary>Short-lived enrollment JWT; only valid on MFA enrollment endpoints.</summary>
    public string? EnrollmentToken { get; init; }

    /// <summary>UTC end of the MFA grace period when the user is within the grace window.</summary>
    public DateTime? MfaGracePeriodEndsAt { get; init; }

    // -------------------------------------------------------------------------
    // Factory methods
    // -------------------------------------------------------------------------

    public static FirstFactorResult Success(Guid userId, bool mfaEnabled)
        => new() { Succeeded = true, UserId = userId, MfaEnabled = mfaEnabled };

    public static FirstFactorResult Locked(Guid userId, DateTime lockoutEnd)
        => new() { Succeeded = false, UserId = userId, IsLockedOut = true, LockoutEnd = lockoutEnd };

    public static FirstFactorResult Failed()
        => new() { Succeeded = false };

    public static FirstFactorResult MfaRequired(Guid userId, MfaChallengeResult challenge)
        => new() { Succeeded = true, UserId = userId, MfaEnabled = true, Challenge = challenge };

    public static FirstFactorResult EnrollmentRequired(Guid userId, string enrollmentToken)
        => new() { Succeeded = true, UserId = userId, RequiresEnrollment = true, EnrollmentToken = enrollmentToken };

    public static FirstFactorResult SuccessInGracePeriod(Guid userId, DateTime gracePeriodEndsAt)
        => new() { Succeeded = true, UserId = userId, MfaGracePeriodEndsAt = gracePeriodEndsAt };
}
