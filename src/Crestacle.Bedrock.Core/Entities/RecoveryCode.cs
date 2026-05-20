namespace Crestacle.Bedrock.Core.Entities;

/// <summary>
/// One of the backup MFA recovery codes. Single-use; never hard-deleted.
/// <see cref="UsedAt"/> provides the audit trail. All codes for a user are replaced together on regeneration.
/// </summary>
public sealed class RecoveryCode
{
    private RecoveryCode() { }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }

    /// <summary>SHA256 hex of the plaintext recovery code; max 128 chars.</summary>
    public string CodeHash { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }

    /// <summary>UTC timestamp when the code was consumed; null means still available.</summary>
    public DateTime? UsedAt { get; private set; }

    public string? TenantId { get; private set; }

    // -------------------------------------------------------------------------
    // Factory
    // -------------------------------------------------------------------------

    public static RecoveryCode Create(Guid userId, string codeHash, string? tenantId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codeHash);

        return new RecoveryCode
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CodeHash = codeHash,
            TenantId = tenantId,
            CreatedAt = DateTime.UtcNow
        };
    }

    // -------------------------------------------------------------------------
    // State methods
    // -------------------------------------------------------------------------

    /// <summary>Marks the code as consumed.</summary>
    public void MarkUsed() => UsedAt = DateTime.UtcNow;

    // -------------------------------------------------------------------------
    // Computed properties
    // -------------------------------------------------------------------------

    /// <summary>Returns true when the code has not yet been consumed.</summary>
    public bool IsAvailable => UsedAt is null;
}
