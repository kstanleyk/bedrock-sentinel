namespace Crestacle.Bedrock.Core.Interfaces;

/// <summary>
/// Delivers transactional emails: email verification, password reset, account lockout
/// notification, and MFA OTP delivery. The default implementation is a no-op that logs
/// a warning; consumers must provide a real implementation for production use.
/// </summary>
public interface IEmailSender
{
    /// <summary>Sends an email containing the account-verification link.</summary>
    /// <param name="toEmail">The recipient's email address.</param>
    /// <param name="verificationUrl">The full verification URL to embed in the message body.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SendEmailVerificationAsync(string toEmail, string verificationUrl, CancellationToken ct = default);

    /// <summary>Sends a password-reset email containing a time-limited reset link.</summary>
    /// <param name="toEmail">The recipient's email address.</param>
    /// <param name="resetUrl">The full password-reset URL to embed in the message body.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SendPasswordResetAsync(string toEmail, string resetUrl, CancellationToken ct = default);

    /// <summary>Notifies the user that their account has been temporarily locked due to failed login attempts.</summary>
    /// <param name="toEmail">The recipient's email address.</param>
    /// <param name="lockoutEnd">The UTC time at which the lockout expires.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SendAccountLockedAsync(string toEmail, DateTime lockoutEnd, CancellationToken ct = default);

    /// <summary>Delivers a one-time password code to the user's email address for MFA verification.</summary>
    /// <param name="toEmail">The recipient's email address.</param>
    /// <param name="code">The plaintext OTP code to deliver.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SendMfaOtpAsync(string toEmail, string code, CancellationToken ct = default);

    /// <summary>Sends a generic transactional email with the specified subject and HTML body.</summary>
    /// <param name="toEmail">The recipient's email address.</param>
    /// <param name="subject">The email subject line.</param>
    /// <param name="htmlBody">The HTML-formatted message body.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default);

    /// <summary>Sends a confirmation link to the new email address to complete an email change request.</summary>
    /// <param name="newEmail">The new (unconfirmed) email address to send the link to.</param>
    /// <param name="confirmationUrl">The full confirmation URL to embed in the message body.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SendEmailChangeVerificationAsync(string newEmail, string confirmationUrl, CancellationToken ct = default);

    /// <summary>Notifies the current (old) email address that an email change has been requested.</summary>
    /// <param name="oldEmail">The current email address to send the security notice to.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SendEmailChangeNotificationAsync(string oldEmail, CancellationToken ct = default);

    /// <summary>Sends a magic-link login email containing a time-limited, single-use authentication link.</summary>
    /// <param name="toEmail">The recipient's email address.</param>
    /// <param name="magicLinkUrl">The full magic-link URL to embed in the message body.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SendMagicLinkAsync(string toEmail, string magicLinkUrl, CancellationToken ct = default);
}
