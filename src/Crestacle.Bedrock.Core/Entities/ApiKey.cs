using System.Security.Cryptography;
using System.Text;

namespace Crestacle.Bedrock.Core.Entities;

/// <summary>
/// An API key that authenticates machine-to-machine requests via the X-Api-Key header.
/// The raw key is returned only once at creation; only a SHA256 hash is stored.
/// </summary>
public sealed class ApiKey
{
    private ApiKey() { }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }

    /// <summary>SHA256 hex of the raw key; max 64 chars; unique index.</summary>
    public string KeyHash { get; private set; } = string.Empty;

    /// <summary>First 8 chars of the raw key (e.g. "bdrk_XYZ") shown in listings.</summary>
    public string Prefix { get; private set; } = string.Empty;

    /// <summary>Optional human-readable label; max 100 chars.</summary>
    public string? Name { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? LastUsedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? TenantId { get; private set; }

    // -------------------------------------------------------------------------
    // Factory — returns entity + plaintext key (caller must deliver rawKey to user)
    // -------------------------------------------------------------------------

    public static (ApiKey Entity, string RawKey) Create(
        Guid userId,
        string? name,
        DateTime? expiresAt = null,
        string? tenantId = null)
    {
        var rawBytes = RandomNumberGenerator.GetBytes(32);
        var rawKey = "bdrk_" + Convert.ToBase64String(rawBytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

        var keyHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)))
            .ToLowerInvariant();

        var entity = new ApiKey
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            KeyHash = keyHash,
            Prefix = rawKey[..8],
            Name = name,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            TenantId = tenantId
        };

        return (entity, rawKey);
    }

    // -------------------------------------------------------------------------
    // State methods
    // -------------------------------------------------------------------------

    public void Revoke() => RevokedAt = DateTime.UtcNow;

    // -------------------------------------------------------------------------
    // Computed properties
    // -------------------------------------------------------------------------

    public bool IsActive => RevokedAt is null && (ExpiresAt is null || DateTime.UtcNow < ExpiresAt.Value);
}
