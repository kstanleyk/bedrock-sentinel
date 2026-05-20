using Crestacle.Bedrock.AspNetCore.Services;
using Crestacle.Bedrock.Core.Enumerations;
using Crestacle.Bedrock.Core.Exceptions;
using Crestacle.Bedrock.Core.Interfaces.Services;
using Crestacle.Bedrock.Core.Options;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Crestacle.Bedrock.Tests.Unit.Services;

public sealed class OtpSendLimiterTests
{
    private sealed class InMemoryBedrockCache : IBedrockCache
    {
        private readonly Dictionary<string, string> _store = new();

        public Task SetAsync(string key, string value, TimeSpan expiry, CancellationToken ct = default)
        {
            _store[key] = value;
            return Task.CompletedTask;
        }

        public Task<string?> GetAsync(string key, CancellationToken ct = default)
            => Task.FromResult(_store.TryGetValue(key, out var v) ? v : null);

        public Task RemoveAsync(string key, CancellationToken ct = default)
        {
            _store.Remove(key);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string key, CancellationToken ct = default)
            => Task.FromResult(_store.ContainsKey(key));

        public Task<bool> TryAcquireLockAsync(string key, TimeSpan ttl, CancellationToken ct = default)
            => Task.FromResult(true);

        public Task ReleaseLockAsync(string key, CancellationToken ct = default)
            => Task.CompletedTask;
    }

    private static OtpSendLimiter CreateLimiter(int maxSends = 5) =>
        new(
            new InMemoryBedrockCache(),
            Options.Create(new BedrockOptions
            {
                Jwt = { SigningKey = "test-key-that-is-long-enough-32b!" },
                Otp = new OtpOptions { MaxSendsPerWindow = maxSends, SendWindow = TimeSpan.FromMinutes(10) }
            }));

    [Fact]
    public async Task GuardAsync_WithinLimit_Succeeds()
    {
        var limiter = CreateLimiter(5);
        var userId = Guid.NewGuid();

        for (var i = 0; i < 5; i++)
            await limiter.GuardAsync(userId, OtpPurpose.Login);
    }

    [Fact]
    public async Task GuardAsync_SixthRequest_ThrowsRateLimitException()
    {
        var limiter = CreateLimiter(5);
        var userId = Guid.NewGuid();

        for (var i = 0; i < 5; i++)
            await limiter.GuardAsync(userId, OtpPurpose.Login);

        var act = () => limiter.GuardAsync(userId, OtpPurpose.Login);
        await act.Should().ThrowAsync<BedrockRateLimitException>();
    }

    [Fact]
    public async Task GuardAsync_DifferentPurposes_TrackSeparately()
    {
        var limiter = CreateLimiter(1);
        var userId = Guid.NewGuid();

        await limiter.GuardAsync(userId, OtpPurpose.Login);
        // StepUp has its own counter — this must not throw
        await limiter.GuardAsync(userId, OtpPurpose.StepUp);
    }

    [Fact]
    public async Task GuardAsync_DifferentUsers_TrackSeparately()
    {
        var limiter = CreateLimiter(1);
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();

        await limiter.GuardAsync(user1, OtpPurpose.Login);
        // user2 has a fresh counter — this must not throw
        await limiter.GuardAsync(user2, OtpPurpose.Login);
    }

    [Fact]
    public async Task GuardAsync_ExceedLimit_RetryAfterMatchesWindow()
    {
        var window = TimeSpan.FromMinutes(7);
        var limiter = new OtpSendLimiter(
            new InMemoryBedrockCache(),
            Options.Create(new BedrockOptions
            {
                Jwt = { SigningKey = "test-key-that-is-long-enough-32b!" },
                Otp = new OtpOptions { MaxSendsPerWindow = 1, SendWindow = window }
            }));

        var userId = Guid.NewGuid();
        await limiter.GuardAsync(userId, OtpPurpose.Login);

        var ex = await Assert.ThrowsAsync<BedrockRateLimitException>(
            () => limiter.GuardAsync(userId, OtpPurpose.Login));

        ex.RetryAfter.Should().Be(window);
    }
}
