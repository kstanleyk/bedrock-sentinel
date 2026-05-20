namespace Crestacle.Bedrock.Core.Entities;

/// <summary>
/// A time-limited, single-use invitation that lets an admin pre-register an email address.
/// Accepting the invitation creates a credential and auto-confirms the email.
/// </summary>
public sealed class Invitation
{
    private Invitation() { }

    public Guid Id { get; private set; }

    /// <summary>SHA256 hex of the raw token embedded in the invitation link; max 64 chars.</summary>
    public string TokenHash { get; private set; } = string.Empty;

    /// <summary>The email address this invitation was issued for; max 256 chars.</summary>
    public string TargetEmail { get; private set; } = string.Empty;

    /// <summary>UserId of the admin who created the invitation; null for system-generated invitations.</summary>
    public Guid? InvitedByUserId { get; private set; }

    /// <summary>Optional hint for the consuming application to assign a role on acceptance; max 100 chars.</summary>
    public string? RoleHint { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }

    /// <summary>UTC timestamp when the invitation was accepted; null means not yet accepted.</summary>
    public DateTime? AcceptedAt { get; private set; }

    public string? TenantId { get; private set; }

    // -------------------------------------------------------------------------
    // Factory
    // -------------------------------------------------------------------------

    public static Invitation Create(
        string tokenHash,
        string targetEmail,
        Guid? invitedByUserId,
        string? roleHint,
        DateTime expiresAt,
        string? tenantId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetEmail);

        return new Invitation
        {
            Id = Guid.NewGuid(),
            TokenHash = tokenHash,
            TargetEmail = targetEmail,
            InvitedByUserId = invitedByUserId,
            RoleHint = roleHint,
            ExpiresAt = expiresAt,
            TenantId = tenantId,
            CreatedAt = DateTime.UtcNow
        };
    }

    // -------------------------------------------------------------------------
    // State methods
    // -------------------------------------------------------------------------

    /// <summary>Marks the invitation as accepted.</summary>
    public void Accept() => AcceptedAt = DateTime.UtcNow;

    // -------------------------------------------------------------------------
    // Computed properties
    // -------------------------------------------------------------------------

    /// <summary>Returns true when the invitation has not been accepted and has not expired.</summary>
    public bool IsValid => AcceptedAt is null && DateTime.UtcNow < ExpiresAt;
}
