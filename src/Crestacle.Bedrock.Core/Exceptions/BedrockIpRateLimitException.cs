using Crestacle.Bedrock.Core;

namespace Crestacle.Bedrock.Core.Exceptions;

/// <summary>
/// Thrown when too many failed login attempts have originated from the same IP block.
/// Maps to HTTP 429 Too Many Requests. The <see cref="RetryAfter"/> value is used to
/// populate the <c>Retry-After</c> response header.
/// </summary>
public sealed class BedrockIpRateLimitException : BedrockException
{
    /// <summary>How long the caller should wait before retrying.</summary>
    public TimeSpan RetryAfter { get; }

    public BedrockIpRateLimitException(TimeSpan retryAfter)
        : base($"Too many failed login attempts from this IP address. Retry after {(int)retryAfter.TotalSeconds} seconds.", BedrockErrorCodes.IpRateLimited)
    {
        RetryAfter = retryAfter;
    }
}
