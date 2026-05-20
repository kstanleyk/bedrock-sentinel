namespace Crestacle.Sentinel.Core.Interfaces;

/// <summary>
/// Short-lived cache for resolved permission sets.
/// Sentinel ships a default in-memory implementation.
/// Replace with a distributed implementation (e.g. Redis) for multi-instance deployments.
/// </summary>
public interface IPermissionCache
{
    Task<HashSet<string>?> GetAsync(string identityId, CancellationToken ct = default);
    Task SetAsync(string identityId, HashSet<string> permissions, CancellationToken ct = default);
    Task InvalidateUserAsync(string identityId, CancellationToken ct = default);
}
