using Crestacle.Bedrock.Core.Interfaces.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Crestacle.Bedrock.AspNetCore.HealthChecks;

/// <summary>
/// Verifies the Bedrock cache by performing a SetAsync / GetAsync / RemoveAsync round-trip
/// on a sentinel key. Any exception is reported as <see cref="HealthStatus.Unhealthy"/>.
/// A no-op cache (e.g. <c>NullBedrockCache</c>) completes without throwing and is reported
/// as <see cref="HealthStatus.Healthy"/>.
/// </summary>
public sealed class BedrockCacheHealthCheck : IHealthCheck
{
    private const string ProbeKey = "Bedrock:health-probe";
    private readonly IBedrockCache _cache;

    public BedrockCacheHealthCheck(IBedrockCache cache) => _cache = cache;

    /// <inheritdoc/>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _cache.SetAsync(ProbeKey, "1", TimeSpan.FromSeconds(5), cancellationToken);
            await _cache.GetAsync(ProbeKey, cancellationToken);
            await _cache.RemoveAsync(ProbeKey, cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Bedrock cache is unreachable.", ex);
        }
    }
}
