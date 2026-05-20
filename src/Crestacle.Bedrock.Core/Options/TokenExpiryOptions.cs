namespace Crestacle.Bedrock.Core.Options;

/// <summary>Expiry settings for short-lived tokens and codes not covered by <see cref="JwtOptions"/>.</summary>
public sealed class TokenExpiryOptions
{
    /// <summary>Lifetime of challenge JWTs issued after first-factor login. Default: 5 minutes.</summary>
    public TimeSpan ChallengeToken { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>Lifetime of step-up JWTs. Default: 5 minutes.</summary>
    public TimeSpan StepUpToken { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>Lifetime of enrollment JWTs. Default: 15 minutes.</summary>
    public TimeSpan EnrollmentToken { get; set; } = TimeSpan.FromMinutes(15);

    /// <summary>Lifetime of email verification tokens. Default: 24 hours.</summary>
    public TimeSpan EmailVerificationToken { get; set; } = TimeSpan.FromHours(24);

    /// <summary>Lifetime of password-reset tokens. Default: 1 hour.</summary>
    public TimeSpan PasswordResetToken { get; set; } = TimeSpan.FromHours(1);

    /// <summary>Lifetime of OTP codes (email and SMS). Default: 10 minutes.</summary>
    public TimeSpan OtpCode { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>Lifetime of MFA challenge records. Default: 5 minutes.</summary>
    public TimeSpan MfaChallenge { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>Lifetime of email change tokens. Default: 24 hours.</summary>
    public TimeSpan EmailChangeToken { get; set; } = TimeSpan.FromHours(24);

    /// <summary>Lifetime of magic-link login tokens. Default: 15 minutes.</summary>
    public TimeSpan MagicLinkToken { get; set; } = TimeSpan.FromMinutes(15);

    /// <summary>Lifetime of admin-issued invitation tokens. Default: 72 hours.</summary>
    public TimeSpan Invitation { get; set; } = TimeSpan.FromHours(72);
}
