using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Crestacle.Bedrock.EntityFramework.HealthChecks;

/// <summary>
/// Verifies database connectivity by calling
/// <see cref="Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade.CanConnectAsync"/>.
/// Returns <see cref="HealthStatus.Unhealthy"/> when the database is unreachable or
/// <c>CanConnectAsync</c> returns <see langword="false"/>.
/// </summary>
public sealed class BedrockDbHealthCheck : IHealthCheck
{
    private readonly BedrockContext _context;

    public BedrockDbHealthCheck(BedrockContext context) => _context = context;

    /// <inheritdoc/>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
            return canConnect
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("Bedrock database is unreachable.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Bedrock database health check failed.", ex);
        }
    }
}
