using Crestacle.Sentinel.Core.DTOs;

namespace Crestacle.Sentinel.Core.Interfaces;

public interface IPermissionConflictRepository
{
    Task<IReadOnlyList<PermissionConflictDto>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Returns true when adding <paramref name="newPermissionId"/> to the given role would
    /// violate a Separation of Duties constraint (the role already holds the conflicting permission).
    /// </summary>
    Task<bool> HasConflictAsync(Guid roleId, string newPermissionId, CancellationToken ct = default);

    Task AddConflictAsync(string permissionIdA, string permissionIdB, CancellationToken ct = default);
    Task RemoveConflictAsync(Guid conflictId, CancellationToken ct = default);
}
