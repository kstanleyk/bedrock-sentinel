using Crestacle.Bedrock.Core.Interfaces.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Crestacle.Bedrock.AspNetCore.BackgroundServices;

internal sealed class BedrockExpiryService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BedrockExpiryService> _logger;

    public BedrockExpiryService(IServiceScopeFactory scopeFactory, ILogger<BedrockExpiryService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await Task.Delay(Interval, stoppingToken); }
            catch (OperationCanceledException) { return; }

            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var mfaChallenges = scope.ServiceProvider.GetRequiredService<IMfaChallengeRepository>();
                var stepUpChallenges = scope.ServiceProvider.GetRequiredService<IStepUpChallengeRepository>();
                var otpCodes = scope.ServiceProvider.GetRequiredService<IOtpCodeRepository>();

                await mfaChallenges.ExpireStaleAsync(stoppingToken);
                await stepUpChallenges.ExpireStaleAsync(stoppingToken);
                await otpCodes.ExpireStaleAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BedrockExpiryService encountered an error during expiry sweep.");
            }
        }
    }
}
