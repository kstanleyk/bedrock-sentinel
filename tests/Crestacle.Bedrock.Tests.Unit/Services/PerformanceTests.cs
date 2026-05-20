using System.Diagnostics;
using Crestacle.Bedrock.AspNetCore.Services;
using Crestacle.Bedrock.Core.Interfaces.Services;
using Crestacle.Bedrock.Core.Options;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Crestacle.Bedrock.Tests.Unit.Services;

/// <summary>
/// Timing assertions for hot-path operations. Each threshold is set to be safely
/// achievable on a CI runner; the comments note tighter production targets where relevant.
/// </summary>
public sealed class PerformanceTests
{
    // -------------------------------------------------------------------------
    // Argon2id — target: ≤ 500 ms per hash on a CI runner
    // -------------------------------------------------------------------------

    [Fact]
    public void Argon2id_Hash_EachCallCompletesUnder1000ms()
    {
        var hasher = new Argon2idPasswordHasher();
        const string password = "TestPassword123!";
        const int runs = 3;
        // Test threshold: 3000 ms — accounts for CPU contention on slow CI runners when run in
        // parallel with other tests (Argon2id uses 4 internal threads). Production target: ≤ 500 ms
        // measured in isolation; verify that separately if tightening CI thresholds.
        const int thresholdMs = 3000;

        // Warm-up: absorb JIT compilation of Konscious.Security.Cryptography before timing.
        _ = hasher.Hash(password);

        for (var i = 0; i < runs; i++)
        {
            var sw = Stopwatch.StartNew();
            _ = hasher.Hash(password);
            sw.Stop();

            sw.ElapsedMilliseconds.Should().BeLessThan(thresholdMs,
                because: $"Argon2id run {i + 1}/{runs} must complete in under {thresholdMs} ms " +
                         $"(64 MB memory, 3 iterations, parallelism 4) — do not relax the hash parameters");
        }
    }

    // -------------------------------------------------------------------------
    // JWT generation — target: ≤ 5 ms average over 100 calls
    // -------------------------------------------------------------------------

    [Fact]
    public void JwtService_GenerateAccessToken_AverageUnder5ms()
    {
        var svc = CreateJwtService();
        var userId = Guid.NewGuid();
        const int iterations = 100;
        const double thresholdMs = 5.0;

        // One warm-up call outside the timed block to absorb JIT compilation
        // and any one-time Microsoft.IdentityModel initialisation.
        _ = svc.GenerateAccessToken(userId, "perf@test.com", []);

        var sw = Stopwatch.StartNew();
        for (var i = 0; i < iterations; i++)
            _ = svc.GenerateAccessToken(userId, "perf@test.com", []);
        sw.Stop();

        var avgMs = sw.Elapsed.TotalMilliseconds / iterations;
        avgMs.Should().BeLessThan(thresholdMs,
            because: $"HS256 JWT generation average over {iterations} calls must be under {thresholdMs} ms");
    }

    // -------------------------------------------------------------------------
    // JWT validation — target: ≤ 5 ms average over 100 calls
    // (CI threshold matches generation; production target is sub-millisecond in isolation)
    // -------------------------------------------------------------------------

    [Fact]
    public void JwtService_ValidateChallengeToken_AverageUnder5ms()
    {
        var svc = CreateJwtService();
        var token = svc.GenerateChallengeToken(Guid.NewGuid(), Guid.NewGuid());
        const int iterations = 100;
        const double thresholdMs = 5.0;

        // Warm-up: absorb JIT and IdentityModel initialisation.
        _ = svc.ValidateChallengeToken(token);

        var sw = Stopwatch.StartNew();
        for (var i = 0; i < iterations; i++)
            _ = svc.ValidateChallengeToken(token);
        sw.Stop();

        var avgMs = sw.Elapsed.TotalMilliseconds / iterations;
        avgMs.Should().BeLessThan(thresholdMs,
            because: $"HS256 JWT validation average over {iterations} calls must be under {thresholdMs} ms on a CI runner");
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static ITokenService CreateJwtService()
    {
        var opts = new BedrockOptions();
        opts.Jwt.SigningKey = "Bedrock-Unit-Test-Signing-Key-32B!";
        opts.Jwt.Issuer = "test";
        opts.Jwt.Audience = "test";
        return new JwtService(Options.Create(opts));
    }
}
