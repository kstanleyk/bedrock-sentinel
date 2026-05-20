using Crestacle.Bedrock.Core.Enumerations;

namespace Crestacle.Bedrock.Core.Entities;

/// <summary>
/// Single-use OTP code issued for a specific purpose. Prior codes for the same user and
/// purpose are invalidated on resend via <see cref="Invalidate"/>.
/// </summary>
public sealed class OtpCode
{
    private OtpCode() { }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public OtpPurpose Purpose { get; private set; }

    /// <summary>SHA256 hex of the plaintext code; max 128 chars.</summary>
    public string CodeHash { get; private set; } = string.Empty;

    public DateTime ExpiresAt { get; private set; }

    /// <summary>UTC timestamp when the code was verified; null means not yet used.</summary>
    public DateTime? UsedAt { get; private set; }

    /// <summary>UTC timestamp when the code was superseded by a resend; null means not invalidated.</summary>
    public DateTime? InvalidatedAt { get; private set; }

    public string? TenantId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // -------------------------------------------------------------------------
    // Factory
    // -------------------------------------------------------------------------

    public static OtpCode Create(
        Guid userId,
        OtpPurpose purpose,
        string codeHash,
        DateTime expiresAt,
        string? tenantId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codeHash);

        return new OtpCode
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Purpose = purpose,
            CodeHash = codeHash,
            ExpiresAt = expiresAt,
            TenantId = tenantId,
            CreatedAt = DateTime.UtcNow
        };
    }

    // -------------------------------------------------------------------------
    // State methods
    // -------------------------------------------------------------------------

    /// <summary>Marks the code as consumed after successful verification.</summary>
    public void MarkUsed() => UsedAt = DateTime.UtcNow;

    /// <summary>Invalidates the code when a newer code is issued for the same user and purpose.</summary>
    public void Invalidate() => InvalidatedAt = DateTime.UtcNow;

    // -------------------------------------------------------------------------
    // Computed properties
    // -------------------------------------------------------------------------

    /// <summary>Returns true when the code is neither used nor invalidated and has not expired.</summary>
    public bool IsActive => UsedAt is null && InvalidatedAt is null && DateTime.UtcNow < ExpiresAt;
}
