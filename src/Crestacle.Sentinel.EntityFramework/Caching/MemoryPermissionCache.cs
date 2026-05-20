using Crestacle.Sentinel.Core.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Crestacle.Sentinel.EntityFramework.Caching;

/// <summary>
/// Default in-process permission cache backed by IMemoryCache.
/// Safe for single-server deployments; replace with a distributed implementation for multi-instance.
/// </summary>
internal sealed class MemoryPermissionCache(IMemoryCache memoryCache, SentinelOptions options) : IPermissionCache
{
    private static string Key(string identityId) => $"sentinel:perms:{identityId}";

    public Task<HashSet<string>?> GetAsync(string identityId, CancellationToken ct = default)
    {
        memoryCache.TryGetValue(Key(identityId), out HashSet<string>? perms);
        return Task.FromResult(perms);
    }

    public Task SetAsync(string identityId, HashSet<string> permissions, CancellationToken ct = default)
    {
        // TimeSpan.Zero means "no caching" — every request goes to the database.
        if (options.PermissionCacheTtl > TimeSpan.Zero)
            memoryCache.Set(Key(identityId), permissions, options.PermissionCacheTtl);
        return Task.CompletedTask;
    }

    public Task InvalidateUserAsync(string identityId, CancellationToken ct = default)
    {
        memoryCache.Remove(Key(identityId));
        return Task.CompletedTask;
    }
}
