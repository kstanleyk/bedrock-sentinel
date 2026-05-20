namespace Crestacle.Sentinel.Core.Entities;

/// <summary>
/// Defines two permissions that must never coexist on the same role (Separation of Duties).
/// For example: Invoice.Create and Invoice.Approve should not be held by the same role
/// to prevent a single user from both raising and self-approving fraudulent transactions.
/// </summary>
public sealed class PermissionConflict : Entity<Guid>
{
    /// <summary>Lexicographically first permission ID in the conflict pair.</summary>
    public string PermissionIdA { get; private set; } = string.Empty;

    /// <summary>Lexicographically second permission ID in the conflict pair.</summary>
    public string PermissionIdB { get; private set; } = string.Empty;

    public string    CreatedBy { get; private set; } = string.Empty;
    public DateTime  CreatedOn { get; private set; }
    public string?   TenantId  { get; private set; }

    private PermissionConflict() { }

    public static PermissionConflict Create(
        string  permissionIdA,
        string  permissionIdB,
        string  createdBy,
        string? tenantId = null)
    {
        // Normalize: always store lexicographically smaller ID as A so duplicate detection is reliable.
        var (a, b) = string.Compare(permissionIdA, permissionIdB, StringComparison.Ordinal) <= 0
            ? (permissionIdA, permissionIdB)
            : (permissionIdB, permissionIdA);

        return new()
        {
            Id            = Guid.NewGuid(),
            PermissionIdA = a,
            PermissionIdB = b,
            CreatedBy     = createdBy,
            CreatedOn     = DateTime.UtcNow,
            TenantId      = tenantId,
        };
    }
}
