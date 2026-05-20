namespace Crestacle.Bedrock.Core.Entities;

/// <summary>
/// Hash-based single-use token sent to a user's new email address to confirm an email change.
/// Prior tokens for the same user are invalidated on every new request.
/// </summary>
public sealed class EmailChangeToken
{
    private EmailChangeToken() { }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }

    /// <summary>SHA256 hex of the raw token; max 64 chars.</summary>
    public string TokenHash { get; private set; } = string.Empty;

    /// <summary>The new email address this token authorises; max 256 chars.</summary>
    public string NewEmail { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }

    /// <summary>UTC consumption timestamp; null means the token is still valid.</summary>
    public DateTime? UsedAt { get; private set; }

    public string? TenantId { get; private set; }

    // -------------------------------------------------------------------------
    // Factory
    // -------------------------------------------------------------------------

    public static EmailChangeToken Create(
        Guid userId,
        string tokenHash,
        string newEmail,
        DateTime expiresAt,
        string? tenantId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(newEmail);

        return new EmailChangeToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            NewEmail = newEmail,
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
