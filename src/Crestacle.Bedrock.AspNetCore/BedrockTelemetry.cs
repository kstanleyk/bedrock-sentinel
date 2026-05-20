using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Crestacle.Bedrock.AspNetCore;

/// <summary>
/// Central telemetry entry point for Bedrock distributed tracing and metrics.
/// </summary>
/// <remarks>
/// <para>
/// Consumers using OpenTelemetry can subscribe to Bedrock spans and meters by adding the
/// source/meter name to their provider builders:
/// <code>
/// builder.Services.AddOpenTelemetry()
///     .WithTracing(tracing => tracing.AddSource("Crestacle.Bedrock"))
///     .WithMetrics(metrics => metrics.AddMeter("Crestacle.Bedrock"));
/// </code>
/// When no listener is attached both the <see cref="ActivitySource"/> and the <see cref="Meter"/>
/// are no-ops and incur no measurable overhead.
/// </para>
/// </remarks>
public static class BedrockTelemetry
{
    /// <summary>
    /// The <see cref="System.Diagnostics.ActivitySource"/> for all Bedrock spans.
    /// Source name: <c>Crestacle.Bedrock</c>.
    /// </summary>
    public static readonly ActivitySource ActivitySource =
        new("Crestacle.Bedrock", "1.0.0");

    /// <summary>
    /// The <see cref="System.Diagnostics.Metrics.Meter"/> for all Bedrock counters.
    /// Meter name: <c>Crestacle.Bedrock</c>.
    /// </summary>
    public static readonly Meter Meter = new("Crestacle.Bedrock", "1.0.0");

    // ── Authentication ────────────────────────────────────────────────────────

    /// <summary>Total login attempts (all outcomes).</summary>
    public static readonly Counter<long> LoginAttempts =
        Meter.CreateCounter<long>("bedrock.login.attempts", description: "Total login attempts.");

    /// <summary>Successful first-factor logins (may still require MFA).</summary>
    public static readonly Counter<long> LoginSuccesses =
        Meter.CreateCounter<long>("bedrock.login.successes", description: "Successful first-factor logins.");

    /// <summary>Failed login attempts (bad credentials, locked, unconfirmed, etc.).</summary>
    public static readonly Counter<long> LoginFailures =
        Meter.CreateCounter<long>("bedrock.login.failures", description: "Failed login attempts.");

    /// <summary>Accounts locked out after exceeding the failed-attempt threshold.</summary>
    public static readonly Counter<long> AccountLockouts =
        Meter.CreateCounter<long>("bedrock.account.lockouts", description: "Account lockout events.");

    /// <summary>New user registrations.</summary>
    public static readonly Counter<long> Registrations =
        Meter.CreateCounter<long>("bedrock.registrations", description: "New user registrations.");

    // ── Tokens ────────────────────────────────────────────────────────────────

    /// <summary>Successful refresh token rotations.</summary>
    public static readonly Counter<long> TokenRefreshes =
        Meter.CreateCounter<long>("bedrock.token.refreshes", description: "Successful refresh token rotations.");

    /// <summary>Failed refresh token rotation attempts (invalid, expired, reuse).</summary>
    public static readonly Counter<long> TokenRefreshFailures =
        Meter.CreateCounter<long>("bedrock.token.refresh.failures", description: "Failed refresh token rotations.");

    // ── MFA ──────────────────────────────────────────────────────────────────

    /// <summary>MFA challenges issued.</summary>
    public static readonly Counter<long> MfaChallengesIssued =
        Meter.CreateCounter<long>("bedrock.mfa.challenges.issued", description: "MFA challenges issued.");

    /// <summary>Successful MFA verifications.</summary>
    public static readonly Counter<long> MfaSuccesses =
        Meter.CreateCounter<long>("bedrock.mfa.successes", description: "Successful MFA verifications.");

    // ── Rate limiting ─────────────────────────────────────────────────────────

    /// <summary>IP-based rate limit trips on the login endpoint.</summary>
    public static readonly Counter<long> IpRateLimitTrips =
        Meter.CreateCounter<long>("bedrock.ratelimit.ip.trips", description: "IP rate limit trips on the login endpoint.");

    /// <summary>OTP send-rate-limit trips.</summary>
    public static readonly Counter<long> OtpRateLimitTrips =
        Meter.CreateCounter<long>("bedrock.ratelimit.otp.trips", description: "OTP send-rate-limit trips.");
}
