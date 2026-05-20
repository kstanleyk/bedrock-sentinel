namespace Crestacle.Bedrock.Core.Entities;

/// <summary>
/// Immutable record of a user's acceptance of a specific policy version.
/// Never updated or deleted — append-only consent audit trail.
/// </summary>
public sealed class ConsentRecord
{
    private ConsentRecord() { }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }

    /// <summary>Identifies the policy category, e.g. "TermsOfService" or "PrivacyPolicy"; max 50 chars.</summary>
    public string PolicyType { get; private set; } = string.Empty;

    /// <summary>Version string of the accepted policy document; max 20 chars.</summary>
    public string PolicyVersion { get; private set; } = string.Empty;

    public DateTime AcceptedAt { get; private set; }

    /// <summary>Client IP address at the time of acceptance; max 45 chars.</summary>
    public string IpAddress { get; private set; } = string.Empty;

    /// <summary>Client User-Agent header at the time of acceptance; max 512 chars.</summary>
    public string UserAgent { get; private set; } = string.Empty;

    public string? TenantId { get; private set; }

    // -------------------------------------------------------------------------
    // Factory
    // -------------------------------------------------------------------------

    public static ConsentRecord Create(
        Guid userId,
        string policyType,
        string policyVersion,
        string ipAddress,
        string userAgent,
        string? tenantId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(policyType);
        ArgumentException.ThrowIfNullOrWhiteSpace(policyVersion);
        ArgumentException.ThrowIfNullOrWhiteSpace(ipAddress);
        ArgumentException.ThrowIfNullOrWhiteSpace(userAgent);

        return new ConsentRecord
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PolicyType = policyType,
            PolicyVersion = policyVersion,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            TenantId = tenantId,
            AcceptedAt = DateTime.UtcNow
        };
    }
}
