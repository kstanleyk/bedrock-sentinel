using System.Security.Cryptography;
using System.Text;
using Crestacle.Bedrock.Core.Entities;
using Crestacle.Bedrock.Core.Interfaces.Repositories;
using Crestacle.Bedrock.Core.Interfaces.Services;
using Crestacle.Bedrock.Core.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Crestacle.Bedrock.AspNetCore.Services;

internal sealed partial class AnomalyDetector : IAnomalyDetector
{
    private const string IpBlockKeyPrefix = "Bedrock:ipblock:";

    private readonly IKnownDeviceRepository _deviceRepo;
    private readonly IBedrockCache _cache;
    private readonly BedrockOptions _options;
    private readonly ILogger<AnomalyDetector> _logger;

    public AnomalyDetector(
        IKnownDeviceRepository deviceRepo,
        IBedrockCache cache,
        IOptions<BedrockOptions> options,
        ILogger<AnomalyDetector> logger)
    {
        _deviceRepo = deviceRepo;
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    public string ComputeFingerprint(string userAgent, string ipAddress)
    {
        var ipBlock = ExtractIpBlock(ipAddress);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"{userAgent}|{ipBlock}"));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public string ExtractIpBlock(string ipAddress)
    {
        if (string.IsNullOrEmpty(ipAddress))
            return "unknown";

        var v4Parts = ipAddress.Split('.');
        if (v4Parts.Length == 4)
            return $"{v4Parts[0]}.{v4Parts[1]}";

        // IPv6: take first two non-empty groups
        var v6Parts = ipAddress.Split(':');
        var meaningful = v6Parts.Where(p => !string.IsNullOrEmpty(p)).Take(2).ToArray();
        return meaningful.Length >= 2 ? $"{meaningful[0]}:{meaningful[1]}" : ipAddress;
    }

    public async Task<bool> IsAnomalousAsync(
        Guid userId,
        string fingerprint,
        string ipBlock,
        CancellationToken ct = default)
    {
        // No baseline yet (first login ever) — not anomalous
        var knownDevices = await _deviceRepo.GetByUserAsync(userId, ct);
        if (knownDevices.Count > 0 && !knownDevices.Any(d => d.FingerprintHash == fingerprint))
        {
            LogAnomalyDetected(_logger, userId, fingerprint, ipBlock);
            return true;
        }

        // Rapid IP block change within the detection window
        var cachedIpBlock = await _cache.GetAsync($"{IpBlockKeyPrefix}{userId}", ct);
        if (cachedIpBlock is not null && cachedIpBlock != ipBlock)
        {
            LogAnomalyDetected(_logger, userId, fingerprint, ipBlock);
            return true;
        }

        return false;
    }

    public async Task RecordDeviceAsync(
        Guid userId,
        string fingerprint,
        string ipBlock,
        string userAgent,
        CancellationToken ct = default)
    {
        var existing = await _deviceRepo.GetByFingerprintAsync(userId, fingerprint, ct);
        if (existing is not null)
        {
            existing.RecordSeen();
            await _deviceRepo.UpdateAsync(existing, ct);
        }
        else
        {
            await _deviceRepo.AddAsync(KnownDevice.Create(userId, fingerprint, ipBlock, userAgent), ct);
        }

        // Cache update is immediate — no unit-of-work needed
        await _cache.SetAsync(
            $"{IpBlockKeyPrefix}{userId}",
            ipBlock,
            _options.AnomalyDetection.RapidIpChangeWindow,
            ct);
    }

    [LoggerMessage(4001, LogLevel.Warning, "Anomaly detected: userId={UserId} fingerprintHash={FingerprintHash} ipBlock={IpBlock}")]
    private static partial void LogAnomalyDetected(ILogger logger, Guid userId, string fingerprintHash, string ipBlock);
}
