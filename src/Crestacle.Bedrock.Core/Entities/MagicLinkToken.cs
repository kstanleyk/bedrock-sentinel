namespace Crestacle.Bedrock.Core.Entities;

/// <summary>
/// Single-use token sent via email to authenticate a user without a password.
/// Default expiry is 15 minutes.
/// </summary>
public sealed class MagicLinkToken
{
    private MagicLinkToken() { }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }

    /// <summary>SHA256 hex of the raw token; max 64 chars.</summary>
    public string TokenHash { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }

    /// <summary>UTC consumption timestamp; null means the token is still valid.</summary>
    public DateTime? UsedAt { get; private set; }

    /// <summary>IP address from the request that issued the magic link.</summary>
    public string? IpAddress { get; private set; }

    public string? TenantId { get; private set; }

    // -------------------------------------------------------------------------
    // Factory
    // -------------------------------------------------------------------------

    public static MagicLinkToken Create(
        Guid userId,
        string tokenHash,
        DateTime expiresAt,
        string? ipAddress = null,
        string? tenantId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);

        return new MagicLinkToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            IpAddress = ipAddress,
            TenantId = tenantId
        };
    }

    // -------------------------------------------------------------------------
    // State methods
    // -------------------------------------------------------------------------

    public void MarkUsed() => UsedAt = DateTime.UtcNow;

    // -------------------------------------------------------------------------
    // Computed properties
    // -------------------------------------------------------------------------

    public bool IsValid => UsedAt is null && DateTime.UtcNow < ExpiresAt;
}
