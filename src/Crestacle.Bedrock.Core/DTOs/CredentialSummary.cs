using Crestacle.Bedrock.Core.Enumerations;

namespace Crestacle.Bedrock.Core.DTOs;

/// <summary>Compact credential view returned in paginated admin user lists.</summary>
public sealed record CredentialSummary(
    Guid UserId,
    string Email,
    AccountStatus Status,
    bool EmailConfirmed,
    bool MfaEnabled,
    DateTime? LockoutEnd,
    DateTime CreatedAt);
