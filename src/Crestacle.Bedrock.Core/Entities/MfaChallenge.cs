using Crestacle.Bedrock.Core.Enumerations;

namespace Crestacle.Bedrock.Core.Entities;

/// <summary>
/// Short-lived record created after successful first-factor login when 2FA is required.
/// The client must present the challenge token (derived from this record's <see cref="Id"/>)
/// to complete the login.
/// </summary>
public sealed class MfaChallenge
{
    private MfaChallenge() { }

    /// <summary>Primary key; also used as the <c>jti</c> of the challenge JWT.</summary>
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    /// <summary>The method the user must verify with.</summary>
    public MfaMethod Method { get; private set; }

    /// <summary>SHA256 of the OTP code; null for TOTP (code verified on the fly).</summary>
    public string? CodeHash { get; private set; }

    public DateTime ExpiresAt { get; private set; }

    /// <summary>UTC timestamp when the challenge was consumed; null means pending.</summary>
    public DateTime? UsedAt { get; private set; }

    /// <summary>IP address that initiated the login; max 45 chars.</summary>
    public string IpAddress { get; private set; } = string.Empty;

    /// <summary>User-Agent of the initiating request; max 512 chars.</summary>
    public string UserAgent { get; private set; } = string.Empty;

    public string? TenantId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // -------------------------------------------------------------------------
    // Factory
    // -------------------------------------------------------------------------

    public static MfaChallenge Create(
        Guid userId,
        MfaMethod method,
        string? codeHash,
        string ipAddress,
        string userAgent,
        DateTime expiresAt,
        string? tenantId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ipAddress);
        ArgumentException.ThrowIfNullOrWhiteSpace(userAgent);

        return new MfaChallenge
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Method = method,
            CodeHash = codeHash,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            ExpiresAt = expiresAt,
            TenantId = tenantId,
            CreatedAt = DateTime.UtcNow
        };
    }

    // -------------------------------------------------------------------------
    // State methods
    // -------------------------------------------------------------------------

    /// <summary>Marks the challenge as consumed.</summary>
    public void MarkUsed() => UsedAt = DateTime.UtcNow;

    // -------------------------------------------------------------------------
    // Computed properties
    // -------------------------------------------------------------------------

    /// <summary>Returns true when the challenge has not been used and has not expired.</summary>
    public bool IsValid => UsedAt is null && DateTime.UtcNow < ExpiresAt;
}
