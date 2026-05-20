using Crestacle.Bedrock.Core.Interfaces.Services;
using StackExchange.Redis;

namespace Crestacle.Bedrock.Redis;

/// <summary>
/// Redis-backed <see cref="IBedrockCache"/> implementation for distributed deployments.
/// All operations are forwarded to the configured Redis database.
/// <see cref="TryAcquireLockAsync"/> uses <c>SET NX PX</c> for atomic distributed locking.
/// </summary>
public sealed class RedisBedrockCache : IBedrockCache
{
    private readonly IDatabase _db;

    public RedisBedrockCache(IConnectionMultiplexer multiplexer)
    {
        _db = multiplexer.GetDatabase();
    }

    /// <inheritdoc />
    public Task SetAsync(string key, string value, TimeSpan expiry, CancellationToken ct = default)
        => _db.StringSetAsync(key, value, expiry);

    /// <inheritdoc />
    public async Task<string?> GetAsync(string key, CancellationToken ct = default)
    {
        var value = await _db.StringGetAsync(key);
        return value.HasValue ? (string?)value : null;
    }

    /// <inheritdoc />
    public Task RemoveAsync(string key, CancellationToken ct = default)
        => _db.KeyDeleteAsync(key);

    /// <inheritdoc />
    public Task<bool> ExistsAsync(string key, CancellationToken ct = default)
        => _db.KeyExistsAsync(key);

    /// <inheritdoc />
    public Task<bool> TryAcquireLockAsync(string key, TimeSpan ttl, CancellationToken ct = default)
        => _db.StringSetAsync(key, "1", ttl, When.NotExists);

    /// <inheritdoc />
    public Task ReleaseLockAsync(string key, CancellationToken ct = default)
        => _db.KeyDeleteAsync(key);
}
