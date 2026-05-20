namespace Crestacle.Sentinel.Core.Entities;

public sealed class UserRole
{
    /// <summary>Surrogate primary key — allows multiple time-bound assignments for the same (User, Role) pair.</summary>
    public Guid Id       { get; set; }

    public Guid UserId   { get; set; }
    public User User     { get; set; } = null!;

    public Guid RoleId   { get; set; }
    public Role Role     { get; set; } = null!;

    public DateTime  CreatedOn { get; set; }

    /// <summary>
    /// When this assignment expires. Null means the assignment never expires.
    /// Expired assignments are excluded from permission resolution but kept for audit purposes.
    /// </summary>
    public DateTime? ExpiresOn  { get; set; }

    /// <summary>Set when the assignment is soft-deleted. Null means the assignment is active.</summary>
    public DateTime? RemovedOn  { get; set; }

    /// <summary>IdentityId of the actor who removed the assignment.</summary>
    public string?   RemovedBy  { get; set; }

    /// <summary>Tenant scope for this assignment. Null in single-tenant deployments.</summary>
    public string?   TenantId   { get; set; }
}
