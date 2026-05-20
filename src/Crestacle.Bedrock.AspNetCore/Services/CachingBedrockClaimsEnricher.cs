using System.Text.Json;
using Crestacle.Bedrock.Core.Interfaces.Services;
using Crestacle.Bedrock.Core.Options;
using Microsoft.Extensions.Options;

namespace Crestacle.Bedrock.AspNetCore.Services;

/// <summary>
/// Caching decorator for <see cref="IBedrockClaimsEnricher"/>. Stores enricher results in
/// <see cref="IBedrockCache"/> for <see cref="ClaimsCacheOptions.Duration"/> to eliminate
/// one application-DB round-trip per token refresh.
/// </summary>
/// <remarks>
/// Cache keys: <c>Bedrock:claims:{userId}</c> and <c>Bedrock:roles:{userId}</c>.
/// Invalidate early by calling <c>IBedrockCache.RemoveAsync</c> on those keys after a role change.
/// </remarks>
internal sealed class CachingBedrockClaimsEnricher : IBedrockClaimsEnricher
{
    private readonly IBedrockClaimsEnricher _inner;
    private readonly IBedrockCache _cache;
    private readonly TimeSpan _duration;

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public CachingBedrockClaimsEnricher(
        IBedrockClaimsEnricher inner,
        IBedrockCache cache,
        IOptions<BedrockOptions> options)
    {
        _inner = inner;
        _cache = cache;
        _duration = options.Value.ClaimsCache.Duration
            ?? throw new InvalidOperationException(
                "CachingBedrockClaimsEnricher requires ClaimsCacheOptions.Duration to be set.");
    }

    public async Task<IDictionary<string, string>> EnrichAsync(Guid userId, CancellationToken ct = default)
    {
        var key = $"Bedrock:claims:{userId}";
        var cached = await _cache.GetAsync(key, ct);
        if (cached is not null)
            return JsonSerializer.Deserialize<Dictionary<string, string>>(cached, JsonOpts)
                   ?? new Dictionary<string, string>();

        var result = await _inner.EnrichAsync(userId, ct);
        await _cache.SetAsync(key, JsonSerializer.Serialize(result, JsonOpts), _duration, ct);
        return result;
    }

    public async Task<IEnumerable<string>> GetRolesAsync(Guid userId, CancellationToken ct = default)
    {
        var key = $"Bedrock:roles:{userId}";
        var cached = await _cache.GetAsync(key, ct);
        if (cached is not null)
            return JsonSerializer.Deserialize<List<string>>(cached, JsonOpts)
                   ?? [];

        var result = (await _inner.GetRolesAsync(userId, ct)).ToList();
        await _cache.SetAsync(key, JsonSerializer.Serialize(result, JsonOpts), _duration, ct);
        return result;
    }
}
