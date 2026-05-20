namespace Crestacle.Bedrock.Core.Entities;

/// <summary>
/// Per-device session record. Created on every login. Linked to a refresh token by hash.
/// Supports device management: list active sessions, revoke by session ID.
/// </summary>
public sealed class Session
{
    private Session() { }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }

    /// <summary>SHA256 of the current refresh token for this session; updated on rotation.</summary>
    public string TokenHash { get; private set; } = string.Empty;

    /// <summary>SHA256 of User-Agent + IP block; max 128 chars.</summary>
    public string DeviceFingerprint { get; private set; } = string.Empty;

    /// <summary>Client IP address; max 45 chars.</summary>
    public string IpAddress { get; private set; } = string.Empty;

    /// <summary>Client User-Agent header; max 512 chars.</summary>
    public string UserAgent { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }

    /// <summary>UTC timestamp of the last token refresh for this session.</summary>
    public DateTime LastActivityAt { get; private set; }

    public DateTime ExpiresAt { get; private set; }

    /// <summary>UTC revocation timestamp; null means active.</summary>
    public DateTime? RevokedAt { get; private set; }

    /// <summary>IP that performed the revocation; max 45 chars.</summary>
    public string? RevokedByIp { get; private set; }

    public string? TenantId { get; private set; }

    /// <summary>JTI of the access token issued with this session; used for full-logout blacklisting.</summary>
    public string? AccessTokenJti { get; private set; }

    /// <summary>Expiry of the access token issued with this session; used to compute the remaining JTI blacklist TTL on full logout.</summary>
    public DateTime? AccessTokenExpiresAt { get; private set; }

    /// <summary>Optimistic concurrency token.</summary>
    public byte[]? RowVersion { get; private set; }

    // -------------------------------------------------------------------------
    // Factory
    // -------------------------------------------------------------------------

    public static Session Create(
        Guid userId,
        string tokenHash,
        string deviceFingerprint,
        string ipAddress,
        string userAgent,
        DateTime expiresAt,
        string? tenantId = null,
        string? accessTokenJti = null,
        DateTime? accessTokenExpiresAt = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceFingerprint);
        ArgumentException.ThrowIfNullOrWhiteSpace(ipAddress);
        ArgumentException.ThrowIfNullOrWhiteSpace(userAgent);

        var now = DateTime.UtcNow;
        return new Session
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            DeviceFingerprint = deviceFingerprint,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            ExpiresAt = expiresAt,
            CreatedAt = now,
            LastActivityAt = now,
            TenantId = tenantId,
            AccessTokenJti = accessTokenJti,
            AccessTokenExpiresAt = accessTokenExpiresAt
        };
    }

    // -------------------------------------------------------------------------
    // State methods
    // -------------------------------------------------------------------------

    /// <summary>Updates the token hash, last-activity timestamp, and access-token metadata on each token refresh.</summary>
    public void UpdateActivity(string newTokenHash, string? newAccessTokenJti = null, DateTime? newAccessTokenExpiresAt = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newTokenHash);
        TokenHash = newTokenHash;
        LastActivityAt = DateTime.UtcNow;
        if (newAccessTokenJti is not null) AccessTokenJti = newAccessTokenJti;
        if (newAccessTokenExpiresAt is not null) AccessTokenExpiresAt = newAccessTokenExpiresAt;
    }

    /// <summary>Soft-revokes the session.</summary>
    public void Revoke(string byIp)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(byIp);
        RevokedAt = DateTime.UtcNow;
        RevokedByIp = byIp;
    }

    // -------------------------------------------------------------------------
    // Computed properties
    // -------------------------------------------------------------------------

    /// <summary>Returns true when the session has not been revoked and has not expired.</summary>
    public bool IsActive => RevokedAt is null && DateTime.UtcNow < ExpiresAt;
}
