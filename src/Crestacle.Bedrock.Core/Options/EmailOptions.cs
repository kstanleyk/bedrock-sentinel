namespace Crestacle.Bedrock.Core.Options;

/// <summary>
/// Email URL construction settings. Used by credential services to build verification
/// and reset links before passing them to <c>IEmailSender</c>.
/// </summary>
public sealed class EmailOptions
{
    /// <summary>Base URL of the consuming application's frontend (e.g. "https://app.example.com").</summary>
    public string FrontendBaseUrl { get; set; } = string.Empty;

    /// <summary>Path appended to <see cref="FrontendBaseUrl"/> for password reset links.</summary>
    public string PasswordResetPath { get; set; } = "/auth/reset-password";

    /// <summary>Path appended to <see cref="FrontendBaseUrl"/> for email verification links.</summary>
    public string EmailVerificationPath { get; set; } = "/auth/confirm-email";

    /// <summary>Path appended to <see cref="FrontendBaseUrl"/> for email change confirmation links.</summary>
    public string EmailChangePath { get; set; } = "/auth/confirm-email-change";

    /// <summary>Path appended to <see cref="FrontendBaseUrl"/> for magic-link login links.</summary>
    public string MagicLinkPath { get; set; } = "/auth/magic-link/verify";

    /// <summary>Path appended to <see cref="FrontendBaseUrl"/> for invitation acceptance links.</summary>
    public string InvitationPath { get; set; } = "/auth/accept-invitation";
}
