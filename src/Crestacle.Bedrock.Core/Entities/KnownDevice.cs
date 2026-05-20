namespace Crestacle.Bedrock.Core.Entities;

/// <summary>
/// Device fingerprint baseline per user. Used by anomaly detection to identify unknown
/// devices and suspicious IP changes. Updated on each successful authentication.
/// </summary>
public sealed class KnownDevice
{
    private KnownDevice() { }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }

    /// <summary>SHA256 of User-Agent + IP block; max 128 chars.</summary>
    public string FingerprintHash { get; private set; } = string.Empty;

    /// <summary>First two octets of the IP address (e.g. "192.168"); max 16 chars.</summary>
    public string IpBlock { get; private set; } = string.Empty;

    /// <summary>User-Agent string; max 512 chars.</summary>
    public string UserAgent { get; private set; } = string.Empty;

    public DateTime FirstSeenAt { get; private set; }
    public DateTime LastSeenAt { get; private set; }
    public string? TenantId { get; private set; }

    // -------------------------------------------------------------------------
    // Factory
    // -------------------------------------------------------------------------

    public static KnownDevice Create(
        Guid userId,
        string fingerprintHash,
        string ipBlock,
        string userAgent,
        string? tenantId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fingerprintHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(ipBlock);
        ArgumentException.ThrowIfNullOrWhiteSpace(userAgent);

        var now = DateTime.UtcNow;
        return new KnownDevice
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FingerprintHash = fingerprintHash,
            IpBlock = ipBlock,
            UserAgent = userAgent,
            FirstSeenAt = now,
            LastSeenAt = now,
            TenantId = tenantId
        };
    }

    // -------------------------------------------------------------------------
    // State methods
    // -------------------------------------------------------------------------

    /// <summary>Updates <see cref="LastSeenAt"/> to now.</summary>
    public void RecordSeen() => LastSeenAt = DateTime.UtcNow;
}
