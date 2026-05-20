using Crestacle.Bedrock.Core.Enumerations;

namespace Crestacle.Bedrock.Core.DTOs;

/// <summary>Full credential view returned by the admin get-user endpoint.</summary>
public sealed record CredentialDetail(
    Guid UserId,
    string Email,
    AccountStatus Status,
    bool EmailConfirmed,
    bool MfaEnabled,
    MfaMethod? MfaMethod,
    bool IsLockedOut,
    DateTime? LockoutEnd,
    int FailedLoginAttempts,
    DateTime? PasswordExpiresAt,
    DateTime? PasswordChangedAt,
    DateTime? MfaGracePeriodEndsAt,
    string? TenantId,
    DateTime CreatedAt,
    DateTime UpdatedAt);
