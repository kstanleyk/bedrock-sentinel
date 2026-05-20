namespace Crestacle.Bedrock.Core.Options;

/// <summary>
/// Settings for the optional <c>IBedrockClaimsEnricher</c> result cache.
/// When <see cref="Duration"/> is set, Bedrock wraps the registered enricher in a
/// caching decorator that stores the returned claims and roles in <c>IBedrockCache</c>
/// for the specified duration, eliminating one application DB round-trip per token refresh.
/// </summary>
/// <remarks>
/// The cache is keyed per-user (<c>Bedrock:claims:{userId}</c> and
/// <c>Bedrock:roles:{userId}</c>). When a distributed cache (Redis) is configured, the
/// cached data is shared across all pods. Invalidate early by calling
/// <c>IBedrockCache.RemoveAsync("Bedrock:claims:{userId}")</c> after a role change.
/// </remarks>
public sealed class ClaimsCacheOptions
{
    /// <summary>
    /// How long enricher results are cached per user. <c>null</c> (default) disables caching —
    /// the enricher is called on every token issuance and every refresh.
    /// Recommended starting value: <c>TimeSpan.FromMinutes(5)</c>.
    /// </summary>
    public TimeSpan? Duration { get; set; }
}
