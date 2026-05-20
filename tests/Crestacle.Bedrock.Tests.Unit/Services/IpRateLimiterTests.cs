using Crestacle.Bedrock.Core.Exceptions;
using Crestacle.Bedrock.Core.Options;
using FluentAssertions;
using Xunit;

namespace Crestacle.Bedrock.Tests.Unit.Services;

/// <summary>
/// Unit tests for IP rate limiting — exception shape and options defaults.
/// End-to-end flow coverage lives in IpRateLimitTests (integration).
/// </summary>
public sealed class IpRateLimiterTests
{
    // -------------------------------------------------------------------------
    // IpRateLimitOptions defaults
    // -------------------------------------------------------------------------

    [Fact]
    public void IpRateLimitOptions_Defaults_AreCorrect()
    {
        var opts = new IpRateLimitOptions();

        opts.Enabled.Should().BeTrue();
        opts.MaxFailedAttemptsPerIp.Should().Be(100);
        opts.IpLockoutWindow.Should().Be(TimeSpan.FromMinutes(10));
        opts.IpLockoutDuration.Should().Be(TimeSpan.FromMinutes(15));
    }

    // -------------------------------------------------------------------------
    // BedrockIpRateLimitException
    // -------------------------------------------------------------------------

    [Fact]
    public void BedrockIpRateLimitException_SetsRetryAfter()
    {
        var retryAfter = TimeSpan.FromMinutes(15);
        var ex = new BedrockIpRateLimitException(retryAfter);

        ex.RetryAfter.Should().Be(retryAfter);
    }

    [Fact]
    public void BedrockIpRateLimitException_MessageContainsSeconds()
    {
        var retryAfter = TimeSpan.FromMinutes(15);
        var ex = new BedrockIpRateLimitException(retryAfter);

        ex.Message.Should().Contain("900"); // 15 * 60
    }

    [Fact]
    public void BedrockIpRateLimitException_IsBedrockException()
    {
        var ex = new BedrockIpRateLimitException(TimeSpan.FromMinutes(1));
        ex.Should().BeAssignableTo<BedrockException>();
    }

    // -------------------------------------------------------------------------
    // IpRateLimitOptions in BedrockOptions
    // -------------------------------------------------------------------------

    [Fact]
    public void BedrockOptions_IpRateLimit_IsNotNull()
    {
        var opts = new BedrockOptions();
        opts.IpRateLimit.Should().NotBeNull();
    }

    [Fact]
    public void BedrockOptions_IpRateLimit_HasCorrectDefaults()
    {
        var opts = new BedrockOptions();
        opts.IpRateLimit.Enabled.Should().BeTrue();
        opts.IpRateLimit.MaxFailedAttemptsPerIp.Should().Be(100);
    }
}
