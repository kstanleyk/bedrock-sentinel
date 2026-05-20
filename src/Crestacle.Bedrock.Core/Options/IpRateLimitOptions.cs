namespace Crestacle.Bedrock.Core.Options;

/// <summary>IP-based login rate limiting settings.</summary>
public sealed class IpRateLimitOptions
{
    /// <summary>
    /// When <c>true</c>, IP-level rate limiting is enforced in <c>LoginFirstFactorAsync</c>.
    /// Default: <c>true</c>.
    /// </summary>
    /// <remarks>
    /// Consumers may wish to disable this (or bypass at the infrastructure level) for
    /// RFC 1918 / loopback addresses (127.0.0.1, 10.x, 192.168.x) in development
    /// environments where all traffic originates from a single IP.
    /// </remarks>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Number of failed login attempts from the same /16 IP block that triggers a lockout.
    /// Default: 100.
    /// </summary>
    public int MaxFailedAttemptsPerIp { get; set; } = 100;

    /// <summary>
    /// Sliding window over which failed attempts are counted. The counter TTL is reset
    /// on each new failure. Default: 10 minutes.
    /// </summary>
    public TimeSpan IpLockoutWindow { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Duration reported to the caller via <c>Retry-After</c> when the IP is rate-limited.
    /// Default: 15 minutes.
    /// </summary>
    public TimeSpan IpLockoutDuration { get; set; } = TimeSpan.FromMinutes(15);
}
