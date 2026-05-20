namespace Crestacle.Bedrock.Core.Entities;

/// <summary>
/// Hash-based single-use token sent via email to authorise a password reset.
/// Default expiry is 1 hour.
/// </summary>
public sealed class PasswordResetToken
{
    private PasswordResetToken() { }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }

    /// <summary>SHA256 hex of the raw token; max 128 chars.</summary>
    public string TokenHash { get; private set; } = string.Empty;

    public DateTime ExpiresAt { get; private set; }

    /// <summary>UTC consumption timestamp; null means the token is still valid.</summary>
    public DateTime? UsedAt { get; private set; }

    public string? TenantId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // -------------------------------------------------------------------------
    // Factory
    // -------------------------------------------------------------------------

    public static PasswordResetToken Create(
        Guid userId,
        string tokenHash,
        DateTime expiresAt,
        string? tenantId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);

        return new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            TenantId = tenantId,
            CreatedAt = DateTime.UtcNow
        };
    }

    // -------------------------------------------------------------------------
    // State methods
    // -------------------------------------------------------------------------

    /// <summary>Marks the token as consumed.</summary>
    public void MarkUsed() => UsedAt = DateTime.UtcNow;

    // -------------------------------------------------------------------------
    // Computed properties
    // -------------------------------------------------------------------------

    /// <summary>Returns true when the token has not been used and has not expired.</summary>
    public bool IsValid => UsedAt is null && DateTime.UtcNow < ExpiresAt;
}
