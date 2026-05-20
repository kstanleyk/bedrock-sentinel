namespace Crestacle.Bedrock.Core.Options;

/// <summary>OTP send-rate-limiting settings.</summary>
public sealed class OtpOptions
{
    /// <summary>Maximum OTP sends per user per purpose within <see cref="SendWindow"/>. Default: 5.</summary>
    public int MaxSendsPerWindow { get; set; } = 5;

    /// <summary>Sliding window duration for OTP send counting. Default: 10 minutes.</summary>
    public TimeSpan SendWindow { get; set; } = TimeSpan.FromMinutes(10);
}
