using Crestacle.Bedrock.Core;

namespace Crestacle.Bedrock.Core.Exceptions;

/// <summary>
/// Thrown when a rate limit is exceeded (e.g. too many OTP sends within a window).
/// Maps to HTTP 429 Too Many Requests. The <see cref="RetryAfter"/> value is used to
/// populate the <c>Retry-After</c> response header.
/// </summary>
public sealed class BedrockRateLimitException : BedrockException
{
    /// <summary>How long the caller should wait before retrying.</summary>
    public TimeSpan RetryAfter { get; }

    public BedrockRateLimitException(TimeSpan retryAfter)
        : base($"Too many OTP send requests. Retry after {(int)retryAfter.TotalSeconds} seconds.", BedrockErrorCodes.RateLimited)
    {
        RetryAfter = retryAfter;
    }
}
