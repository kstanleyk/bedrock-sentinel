namespace Crestacle.Sentinel.Core.Interfaces;

public interface IUserPermissionRepository : IDisposable
{
    /// <summary>Returns the full set of permission names granted to the user with the given identity ID.</summary>
    Task<HashSet<string>> GetPermissionsForUserAsync(string identityId, CancellationToken ct = default);
}
