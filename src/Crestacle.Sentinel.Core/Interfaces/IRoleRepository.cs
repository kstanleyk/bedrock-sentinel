using Crestacle.Sentinel.Core.DTOs;
using Crestacle.Sentinel.Core.Enums;

namespace Crestacle.Sentinel.Core.Interfaces;

public interface IRoleRepository
{
    Task<PagedResult<RoleDto>> GetAllWithPermissionsAsync(
        int page = 1, int pageSize = 50, string? search = null, CancellationToken ct = default);

    /// <summary>Returns a single role with its active permissions, or null if not found.</summary>
    Task<RoleDto?> GetByIdAsync(Guid roleId, CancellationToken ct = default);

    /// <summary>Returns the RoleType of the given role, or null if the role does not exist.</summary>
    Task<RoleType?> GetRoleTypeAsync(Guid roleId, CancellationToken ct = default);

    /// <summary>Returns true when the given role requires a second admin to approve assignments.</summary>
    Task<bool> RequiresDualApprovalAsync(Guid roleId, CancellationToken ct = default);

    Task AddPermissionAsync(Guid roleId, string permissionId, CancellationToken ct = default);
    Task RemovePermissionAsync(Guid roleId, string permissionId, CancellationToken ct = default);
}
