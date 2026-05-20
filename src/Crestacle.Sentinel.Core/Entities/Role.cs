using Crestacle.Sentinel.Core.Enums;

namespace Crestacle.Sentinel.Core.Entities;

public sealed class Role : Entity<Guid>
{
    public string   Name        { get; private set; } = string.Empty;
    public string   DisplayName { get; private set; } = string.Empty;
    public RoleType Type        { get; private set; }

    /// <summary>
    /// Parent role in the hierarchy. This role inherits all permissions from the parent chain.
    /// Null means the role has no parent.
    /// </summary>
    public Guid?    ParentRoleId          { get; private set; }
    public Role?    ParentRole            { get; private set; }

    /// <summary>
    /// When true, assigning a user to this role requires a second admin to approve
    /// (4-Eyes principle). The assignment is held as a PendingAssignment until reviewed.
    /// </summary>
    public bool     RequiresDualApproval  { get; private set; }

    /// <summary>Tenant this role belongs to. Null for global/shared roles and single-tenant deployments.</summary>
    public string?  TenantId              { get; private set; }

    /// <summary>Optimistic concurrency token — set by the database.</summary>
    public byte[]   RowVersion            { get; private set; } = [];

    public DateTime CreatedOn             { get; private set; }

    public ICollection<UserRole>       UserRoles       { get; private set; } = [];
    public ICollection<RolePermission> RolePermissions { get; private set; } = [];
    public ICollection<Role>           ChildRoles      { get; private set; } = [];

    private Role() { }

    public static Role Create(
        string   name,
        string   displayName,
        RoleType type,
        string?  tenantId            = null,
        bool     requiresDualApproval = false)
        => new()
        {
            Id                   = Guid.NewGuid(),
            Name                 = name,
            DisplayName          = displayName,
            Type                 = type,
            TenantId             = tenantId,
            RequiresDualApproval = requiresDualApproval,
            CreatedOn            = DateTime.UtcNow,
        };

    public void SetDisplayName(string displayName)       => DisplayName          = displayName;
    public void SetParentRole(Guid? parentRoleId)        => ParentRoleId         = parentRoleId;
    public void SetDualApprovalRequired(bool required)   => RequiresDualApproval = required;
}
