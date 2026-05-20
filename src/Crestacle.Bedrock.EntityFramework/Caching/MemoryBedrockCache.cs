using Crestacle.Bedrock.Core.Interfaces.Services;
using Microsoft.Extensions.Caching.Memory;

namespace Crestacle.Bedrock.EntityFramework.Caching;

/// <summary>
/// Process-local <see cref="IBedrockCache"/> backed by <see cref="IMemoryCache"/>.
/// Suitable for single-node deployments and development environments.
/// </summary>
/// <remarks>
/// <para>
/// <b>⚠ Important — single-node only.</b>
/// <see cref="TryAcquireLockAsync"/> uses a process-local <c>lock</c> gate. In a
/// multi-pod deployment (load-balanced, horizontally scaled) this lock does <em>not</em>
/// span pods, so the following behaviours are not guaranteed across nodes:
/// <list type="bullet">
///   <item><c>MaxConcurrentSessions</c> enforcement — two pods may each allow a session
///         to be created simultaneously, silently exceeding the limit.</item>
///   <item>JTI blacklist propagation — a token revoked on one pod is not blacklisted
///         on other pods until its natural expiry.</item>
/// </list>
/// For distributed deployments replace this implementation with one backed by Redis
/// (or another distributed store) that provides atomic NX semantics.
/// </para>
/// </remarks>
public sealed class MemoryBedrockCache : IBedrockCache
{
    private readonly IMemoryCache _cache;
    // Process-local NX gate — see class remarks for multi-pod limitations.
    private readonly object _lockGate = new();

    public MemoryBedrockCache(IMemoryCache cache) => _cache = cache;

    public Task SetAsync(string key, string value, TimeSpan expiry, CancellationToken ct = default)
    {
        try { _cache.Set(key, value, expiry); }
        catch (ObjectDisposedException) { /* cache disposed during shutdown — safe no-op */ }
        return Task.CompletedTask;
    }

    public Task<string?> GetAsync(string key, CancellationToken ct = default)
    {
        try { return Task.FromResult(_cache.Get<string>(key)); }
        catch (ObjectDisposedException) { return Task.FromResult<string?>(null); }
    }

    public Task RemoveAsync(string key, CancellationToken ct = default)
    {
        try { _cache.Remove(key); }
        catch (ObjectDisposedException) { /* cache disposed during shutdown — safe no-op */ }
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key, CancellationToken ct = default)
    {
        try { return Task.FromResult(_cache.TryGetValue(key, out _)); }
        catch (ObjectDisposedException) { return Task.FromResult(false); }
    }

    public Task<bool> TryAcquireLockAsync(string key, TimeSpan ttl, CancellationToken ct = default)
    {
        try
        {
            lock (_lockGate)
            {
                if (_cache.TryGetValue(key, out _))
                    return Task.FromResult(false);

                _cache.Set(key, "1", ttl);
                return Task.FromResult(true);
            }
        }
        catch (ObjectDisposedException) { return Task.FromResult(false); }
    }

    public Task ReleaseLockAsync(string key, CancellationToken ct = default)
    {
        try { _cache.Remove(key); }
        catch (ObjectDisposedException) { /* cache disposed during shutdown — safe no-op */ }
        return Task.CompletedTask;
    }
}
