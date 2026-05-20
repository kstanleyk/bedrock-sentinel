using Crestacle.Bedrock.Core.Enumerations;
using Crestacle.Bedrock.Core.Exceptions;
using Crestacle.Bedrock.Core.Interfaces.Services;
using Crestacle.Bedrock.Core.Options;
using Microsoft.Extensions.Options;

namespace Crestacle.Bedrock.AspNetCore.Services;

/// <summary>
/// Enforces per-user, per-purpose OTP send rate limits using the Bedrock cache.
/// Cache key: <c>Bedrock:otp-send-count:{userId}:{purpose}</c>.
/// </summary>
public sealed class OtpSendLimiter
{
    private const string KeyPrefix = "Bedrock:otp-send-count:";

    private readonly IBedrockCache _cache;
    private readonly OtpOptions _options;

    public OtpSendLimiter(IBedrockCache cache, IOptions<BedrockOptions> options)
    {
        _cache = cache;
        _options = options.Value.Otp;
    }

    /// <summary>
    /// Throws <see cref="BedrockRateLimitException"/> if the user has reached the send limit
    /// for the given purpose within the current window; otherwise increments the counter.
    /// </summary>
    public async Task GuardAsync(Guid userId, OtpPurpose purpose, CancellationToken ct = default)
    {
        var key = $"{KeyPrefix}{userId}:{purpose}";
        var raw = await _cache.GetAsync(key, ct);
        var count = raw is null ? 0 : int.Parse(raw);

        if (count >= _options.MaxSendsPerWindow)
            throw new BedrockRateLimitException(_options.SendWindow);

        await _cache.SetAsync(key, (count + 1).ToString(), _options.SendWindow, ct);
    }
}
