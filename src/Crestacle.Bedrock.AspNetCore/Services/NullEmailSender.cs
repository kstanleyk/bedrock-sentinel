using Crestacle.Bedrock.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Crestacle.Bedrock.AspNetCore.Services;

/// <summary>
/// No-op email sender. Logs a warning on every call to signal that real email delivery
/// has not been configured. Replace with a production implementation in consuming applications.
/// </summary>
public sealed class NullEmailSender : IEmailSender
{
    private readonly ILogger<NullEmailSender> _logger;

    public NullEmailSender(ILogger<NullEmailSender> logger) => _logger = logger;

    public Task SendEmailVerificationAsync(string toEmail, string verificationUrl, CancellationToken ct = default)
    {
        _logger.LogWarning(
            "NullEmailSender: email verification not sent to {Email}. Configure IEmailSender for production use.",
            toEmail);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(string toEmail, string resetUrl, CancellationToken ct = default)
    {
        _logger.LogWarning(
            "NullEmailSender: password reset email not sent to {Email}. Configure IEmailSender for production use.",
            toEmail);
        return Task.CompletedTask;
    }

    public Task SendAccountLockedAsync(string toEmail, DateTime lockoutEnd, CancellationToken ct = default)
    {
        _logger.LogWarning(
            "NullEmailSender: account locked email not sent to {Email}. Configure IEmailSender for production use.",
            toEmail);
        return Task.CompletedTask;
    }

    public Task SendMfaOtpAsync(string toEmail, string code, CancellationToken ct = default)
    {
        _logger.LogWarning(
            "NullEmailSender: MFA OTP not sent to {Email}. Configure IEmailSender for production use.",
            toEmail);
        return Task.CompletedTask;
    }

    public Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
    {
        _logger.LogWarning(
            "NullEmailSender: email '{Subject}' not sent to {Email}. Configure IEmailSender for production use.",
            subject,
            toEmail);
        return Task.CompletedTask;
    }

    public Task SendEmailChangeVerificationAsync(string newEmail, string confirmationUrl, CancellationToken ct = default)
    {
        _logger.LogWarning(
            "NullEmailSender: email change verification not sent to {Email}. Configure IEmailSender for production use.",
            newEmail);
        return Task.CompletedTask;
    }

    public Task SendEmailChangeNotificationAsync(string oldEmail, CancellationToken ct = default)
    {
        _logger.LogWarning(
            "NullEmailSender: email change notification not sent to {Email}. Configure IEmailSender for production use.",
            oldEmail);
        return Task.CompletedTask;
    }

    public Task SendMagicLinkAsync(string toEmail, string magicLinkUrl, CancellationToken ct = default)
    {
        _logger.LogWarning(
            "NullEmailSender: magic link not sent to {Email}. Configure IEmailSender for production use.",
            toEmail);
        return Task.CompletedTask;
    }
}
