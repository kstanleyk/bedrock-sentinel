using Crestacle.Bedrock.Core.Interfaces.Services;

namespace Crestacle.Bedrock.Core.Defaults;

/// <summary>
/// No-op cache that always returns null / false.
/// Used as a fallback when no real <see cref="IBedrockCache"/> implementation is registered
/// (e.g. when running without the EntityFramework package).
/// </summary>
public sealed class NullBedrockCache : IBedrockCache
{
    public Task SetAsync(string key, string value, TimeSpan expiry, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task<string?> GetAsync(string key, CancellationToken ct = default)
        => Task.FromResult<string?>(null);

    public Task RemoveAsync(string key, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task<bool> ExistsAsync(string key, CancellationToken ct = default)
        => Task.FromResult(false);

    public Task<bool> TryAcquireLockAsync(string key, TimeSpan ttl, CancellationToken ct = default)
        => Task.FromResult(true);

    public Task ReleaseLockAsync(string key, CancellationToken ct = default)
        => Task.CompletedTask;
}
