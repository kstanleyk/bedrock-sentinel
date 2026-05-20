namespace Crestacle.Bedrock.Core.Interfaces.Services;

/// <summary>
/// Pluggable cache abstraction. The default implementation uses <c>IMemoryCache</c>;
/// consumers may replace it with a Redis-backed implementation for distributed deployments.
/// </summary>
public interface IBedrockCache
{
    /// <summary>Stores a string value under <paramref name="key"/> with the given sliding expiry.</summary>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The string value to cache.</param>
    /// <param name="expiry">The time-to-live after which the entry is evicted.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SetAsync(string key, string value, TimeSpan expiry, CancellationToken ct = default);

    /// <summary>Returns the cached value for <paramref name="key"/>, or <c>null</c> if the key does not exist or has expired.</summary>
    /// <param name="key">The cache key to look up.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The cached string value, or <c>null</c> when the key is absent or expired.</returns>
    Task<string?> GetAsync(string key, CancellationToken ct = default);

    /// <summary>Removes the cache entry for <paramref name="key"/> if it exists.</summary>
    /// <param name="key">The cache key to remove.</param>
    /// <param name="ct">Cancellation token.</param>
    Task RemoveAsync(string key, CancellationToken ct = default);

    /// <summary>Returns <c>true</c> when the cache contains a non-expired entry for <paramref name="key"/>.</summary>
    /// <param name="key">The cache key to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns><c>true</c> when the key exists and has not expired; <c>false</c> otherwise.</returns>
    Task<bool> ExistsAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Atomically sets <paramref name="key"/> with <paramref name="ttl"/> if the key does not
    /// already exist. Returns <c>true</c> if the lock was acquired, <c>false</c> if it was
    /// already held. Redis deployments should map this to <c>SET NX PX</c>.
    /// </summary>
    /// <param name="key">The lock key to acquire.</param>
    /// <param name="ttl">The time after which the lock is automatically released.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns><c>true</c> when the lock was acquired; <c>false</c> when it was already held.</returns>
    Task<bool> TryAcquireLockAsync(string key, TimeSpan ttl, CancellationToken ct = default);

    /// <summary>Releases a lock previously acquired with <see cref="TryAcquireLockAsync"/>.</summary>
    /// <param name="key">The lock key to release.</param>
    /// <param name="ct">Cancellation token.</param>
    Task ReleaseLockAsync(string key, CancellationToken ct = default);
}
