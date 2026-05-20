using Crestacle.Sentinel.Core.DTOs;

namespace Crestacle.Sentinel.Core.Interfaces;

public interface IUserRepository
{
    Task<PagedResult<UserDto>> GetAllWithRolesAsync(
        int page = 1, int pageSize = 50, string? search = null, CancellationToken ct = default);

    /// <summary>Returns a single user with their active roles, or null if not found.</summary>
    Task<UserDto?> GetByIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Returns the external IdentityId for the given internal user ID, or null if not found.</summary>
    Task<string?> GetIdentityIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Assigns the role to the user. When <paramref name="expiresOn"/> is provided the
    /// assignment is time-bound and will stop granting permissions after that UTC instant.
    /// </summary>
    Task AddRoleAsync(Guid userId, Guid roleId, DateTime? expiresOn = null, CancellationToken ct = default);

    Task RemoveRoleAsync(Guid userId, Guid roleId, CancellationToken ct = default);
}
