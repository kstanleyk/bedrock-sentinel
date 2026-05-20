namespace Crestacle.Bedrock.Core.DTOs;

public sealed record ApiKeySummary(
    Guid Id,
    string Prefix,
    string? Name,
    DateTime CreatedAt,
    DateTime? LastUsedAt,
    DateTime? ExpiresAt,
    bool IsActive);

public sealed record CreateApiKeyResult(
    string RawKey,
    Guid Id,
    string Prefix,
    string? Name,
    DateTime CreatedAt,
    DateTime? ExpiresAt);
