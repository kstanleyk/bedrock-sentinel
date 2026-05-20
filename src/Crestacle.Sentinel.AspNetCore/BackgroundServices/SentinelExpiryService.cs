using Crestacle.Sentinel.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Crestacle.Sentinel.AspNetCore.BackgroundServices;

/// <summary>
/// Background service that periodically marks expired PendingAssignments
/// as <see cref="Core.Enums.AssignmentStatus.Expired"/>.
/// Runs once on startup and then every hour.
/// </summary>
internal sealed class SentinelExpiryService(
    IServiceScopeFactory scopeFactory,
    ILogger<SentinelExpiryService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await SweepExpiredAsync(stoppingToken);
            await Task.Delay(Interval, stoppingToken).ConfigureAwait(false);
        }
    }

    private async Task SweepExpiredAsync(CancellationToken ct)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IPendingAssignmentRepository>();
            await repo.MarkExpiredBatchAsync(ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Sentinel expiry sweep failed.");
        }
    }
}
