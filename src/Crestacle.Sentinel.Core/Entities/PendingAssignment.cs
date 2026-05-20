using Crestacle.Sentinel.Core.Enums;

namespace Crestacle.Sentinel.Core.Entities;

/// <summary>
/// A role assignment that is waiting for a second administrator to approve (4-Eyes principle).
/// Created when a user is assigned to a role that has RequiresDualApproval = true.
/// The actual UserRole row is only created when a different actor approves this request.
/// </summary>
public sealed class PendingAssignment : Entity<Guid>
{
    public Guid   UserId      { get; private set; }
    public Guid   RoleId      { get; private set; }

    /// <summary>IdentityId of the admin who submitted the request.</summary>
    public string RequestedBy  { get; private set; } = string.Empty;
    public DateTime RequestedOn { get; private set; }

    /// <summary>After this time the request auto-expires and can no longer be approved.</summary>
    public DateTime ExpiresOn   { get; private set; }

    /// <summary>
    /// The intended expiry of the UserRole created when this request is approved.
    /// Null means the role assignment will never expire.
    /// </summary>
    public DateTime? RoleExpiresOn { get; private set; }

    public AssignmentStatus Status          { get; private set; }
    public string?          ReviewedBy      { get; private set; }
    public DateTime?        ReviewedOn      { get; private set; }
    public string?          RejectionReason { get; private set; }
    public string?          TenantId        { get; private set; }

    /// <summary>Optimistic concurrency token — prevents two admins from simultaneously approving the same request.</summary>
    public byte[]           RowVersion      { get; private set; } = [];

    private PendingAssignment() { }

    public static PendingAssignment Create(
        Guid      userId,
        Guid      roleId,
        string    requestedBy,
        string?   tenantId      = null,
        DateTime? roleExpiresOn = null,
        int       expiryHours   = 72)
        => new()
        {
            Id            = Guid.NewGuid(),
            UserId        = userId,
            RoleId        = roleId,
            RequestedBy   = requestedBy,
            RequestedOn   = DateTime.UtcNow,
            ExpiresOn     = DateTime.UtcNow.AddHours(expiryHours),
            Status        = AssignmentStatus.Pending,
            TenantId      = tenantId,
            RoleExpiresOn = roleExpiresOn,
        };

    /// <summary>
    /// Marks the assignment as approved and returns true.
    /// Returns false if the request has already been reviewed or has expired.
    /// </summary>
    public bool Approve(string reviewedBy)
    {
        if (Status != AssignmentStatus.Pending || DateTime.UtcNow > ExpiresOn)
            return false;

        Status     = AssignmentStatus.Approved;
        ReviewedBy = reviewedBy;
        ReviewedOn = DateTime.UtcNow;
        return true;
    }

    /// <summary>
    /// Marks the assignment as rejected and returns true.
    /// Returns false if the request has already been reviewed or has expired.
    /// </summary>
    public bool Reject(string reviewedBy, string? reason = null)
    {
        if (Status != AssignmentStatus.Pending || DateTime.UtcNow > ExpiresOn)
            return false;

        Status          = AssignmentStatus.Rejected;
        ReviewedBy      = reviewedBy;
        ReviewedOn      = DateTime.UtcNow;
        RejectionReason = reason;
        return true;
    }

    /// <summary>
    /// Marks the assignment as expired. Called by the background expiry sweep.
    /// Returns false if the assignment is already in a terminal state.
    /// </summary>
    public bool MarkExpired()
    {
        if (Status != AssignmentStatus.Pending)
            return false;

        Status = AssignmentStatus.Expired;
        return true;
    }
}
