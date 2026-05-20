using Crestacle.Bedrock.Core.Enumerations;

namespace Crestacle.Bedrock.Core.Events;

// All domain events are immutable records published after SaveChangesAsync succeeds.

/// <summary>A new credential was registered.</summary>
public sealed record UserRegisteredEvent(
    Guid UserId,
    string Email,
    string? TenantId,
    DateTime OccurredAt);

/// <summary>The user's email address was confirmed.</summary>
public sealed record EmailVerifiedEvent(
    Guid UserId,
    DateTime OccurredAt);

/// <summary>First-factor login succeeded.</summary>
public sealed record LoginSucceededEvent(
    Guid UserId,
    string IpAddress,
    string UserAgent,
    DateTime OccurredAt);

/// <summary>A login attempt failed due to wrong credentials.</summary>
public sealed record LoginFailedEvent(
    string Email,
    string IpAddress,
    DateTime OccurredAt);

/// <summary>An account was locked after exceeding the failed-attempt threshold.</summary>
public sealed record AccountLockedEvent(
    Guid UserId,
    DateTime LockoutEnd,
    string IpAddress,
    DateTime OccurredAt);

/// <summary>An account's lockout was cleared.</summary>
public sealed record AccountUnlockedEvent(
    Guid UserId,
    DateTime OccurredAt);

/// <summary>The user changed their own password.</summary>
public sealed record PasswordChangedEvent(
    Guid UserId,
    DateTime OccurredAt);

/// <summary>A password was reset via a password-reset token.</summary>
public sealed record PasswordResetEvent(
    Guid UserId,
    string IpAddress,
    DateTime OccurredAt);

/// <summary>An MFA method was confirmed and activated.</summary>
public sealed record MfaEnabledEvent(
    Guid UserId,
    MfaMethod Method,
    DateTime OccurredAt);

/// <summary>MFA was disabled on the credential.</summary>
public sealed record MfaDisabledEvent(
    Guid UserId,
    DateTime OccurredAt);

/// <summary>A 2FA challenge was successfully verified.</summary>
public sealed record MfaChallengeSucceededEvent(
    Guid UserId,
    MfaMethod Method,
    DateTime OccurredAt);

/// <summary>A recovery code was consumed.</summary>
public sealed record RecoveryCodeUsedEvent(
    Guid UserId,
    int RemainingCount,
    DateTime OccurredAt);

/// <summary>All recovery codes were regenerated; old codes are now invalidated.</summary>
public sealed record RecoveryCodesRegeneratedEvent(
    Guid UserId,
    DateTime OccurredAt);

/// <summary>A step-up challenge was verified and a step-up JWT was issued.</summary>
public sealed record StepUpCompletedEvent(
    Guid UserId,
    Guid ChallengeId,
    DateTime OccurredAt);

/// <summary>A refresh token was rotated.</summary>
public sealed record TokenRefreshedEvent(
    Guid UserId,
    string IpAddress,
    DateTime OccurredAt);

/// <summary>A refresh token was explicitly revoked.</summary>
public sealed record TokenRevokedEvent(
    Guid UserId,
    DateTime OccurredAt);

/// <summary>An individual session was revoked.</summary>
public sealed record SessionRevokedEvent(
    Guid UserId,
    Guid SessionId,
    DateTime OccurredAt);

/// <summary>An anomalous device fingerprint or IP change was detected.</summary>
public sealed record AnomalyDetectedEvent(
    Guid UserId,
    string FingerprintHash,
    string IpBlock,
    string IpAddress,
    DateTime OccurredAt);
