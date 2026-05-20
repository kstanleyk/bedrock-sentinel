using Crestacle.Bedrock.AspNetCore.HealthChecks;
using Crestacle.Bedrock.Core.Defaults;
using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;

namespace Crestacle.Bedrock.Tests.Unit.HealthChecks;

public sealed class BedrockCacheHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_WithNullCache_ReturnsHealthy()
    {
        var check = new BedrockCacheHealthCheck(new NullBedrockCache());
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(
                "test", check, failureStatus: null, tags: null)
        };

        var result = await check.CheckHealthAsync(context);

        result.Status.Should().Be(HealthStatus.Healthy);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenCacheThrows_ReturnsUnhealthy()
    {
        var check = new BedrockCacheHealthCheck(new ThrowingBedrockCache());
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration(
                "test", check, failureStatus: null, tags: null)
        };

        var result = await check.CheckHealthAsync(context);

        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Exception.Should().NotBeNull();
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private sealed class ThrowingBedrockCache : Crestacle.Bedrock.Core.Interfaces.Services.IBedrockCache
    {
        public Task SetAsync(string key, string value, TimeSpan expiry, CancellationToken ct = default)
            => throw new InvalidOperationException("Cache unavailable.");

        public Task<string?> GetAsync(string key, CancellationToken ct = default)
            => throw new InvalidOperationException("Cache unavailable.");

        public Task RemoveAsync(string key, CancellationToken ct = default)
            => throw new InvalidOperationException("Cache unavailable.");

        public Task<bool> ExistsAsync(string key, CancellationToken ct = default)
            => throw new InvalidOperationException("Cache unavailable.");

        public Task<bool> TryAcquireLockAsync(string key, TimeSpan ttl, CancellationToken ct = default)
            => throw new InvalidOperationException("Cache unavailable.");

        public Task ReleaseLockAsync(string key, CancellationToken ct = default)
            => throw new InvalidOperationException("Cache unavailable.");
    }
}
