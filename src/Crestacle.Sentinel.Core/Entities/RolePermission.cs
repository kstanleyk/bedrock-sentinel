namespace Crestacle.Sentinel.Core.Entities;

public sealed class RolePermission
{
    public Guid   RoleId       { get; set; }
    public Role   Role         { get; set; } = null!;

    public string PermissionId { get; set; } = string.Empty;
    public Permission Permission { get; set; } = null!;

    public DateTime  CreatedOn { get; set; }

    /// <summary>Set when the assignment is soft-deleted. Null means the assignment is active.</summary>
    public DateTime? RemovedOn { get; set; }

    /// <summary>IdentityId of the actor who removed the assignment.</summary>
    public string?   RemovedBy { get; set; }

    /// <summary>Tenant scope for this assignment. Null in single-tenant deployments.</summary>
    public string?   TenantId  { get; set; }
}
