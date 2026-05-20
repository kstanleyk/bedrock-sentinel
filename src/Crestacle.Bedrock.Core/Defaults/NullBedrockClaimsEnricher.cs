using Crestacle.Bedrock.Core.Interfaces.Services;

namespace Crestacle.Bedrock.Core.Defaults;

/// <summary>
/// No-op claims enricher. Returns an empty dictionary, adding no extra claims to access tokens.
/// Replace with a custom implementation to inject application-specific claims.
/// </summary>
public sealed class NullBedrockClaimsEnricher : IBedrockClaimsEnricher
{
    public Task<IDictionary<string, string>> EnrichAsync(Guid userId, CancellationToken ct = default)
        => Task.FromResult<IDictionary<string, string>>(new Dictionary<string, string>());
}
