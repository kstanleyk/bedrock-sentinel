using Crestacle.Bedrock.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Crestacle.Bedrock.AspNetCore.Services;

/// <summary>
/// No-op SMS sender. Logs a warning on every call to signal that real SMS delivery
/// has not been configured. Replace with a production implementation (e.g. Twilio).
/// </summary>
public sealed class NullSmsSender : ISmsSender
{
    private readonly ILogger<NullSmsSender> _logger;

    public NullSmsSender(ILogger<NullSmsSender> logger) => _logger = logger;

    public Task SendOtpAsync(string toPhoneNumber, string code, CancellationToken ct = default)
    {
        _logger.LogWarning(
            "NullSmsSender: OTP not sent to {PhoneNumber}. Configure ISmsSender for production use.",
            toPhoneNumber);
        return Task.CompletedTask;
    }
}
