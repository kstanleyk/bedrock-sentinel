using Crestacle.Bedrock.Core.Interfaces;

namespace Embedded.Infrastructure;

/// <summary>
/// Logs emails to the console instead of sending them.
/// Replace with a real implementation (SMTP, SendGrid, etc.) for production.
/// </summary>
public class DevEmailSender(ILogger<DevEmailSender> logger) : IEmailSender
{
    public Task SendAsync(string to, string subject, string htmlBody,
        CancellationToken ct = default)
    {
        logger.LogInformation("[EMAIL] To: {To} | Subject: {Subject}\n{Body}",
            to, subject, htmlBody);
        return Task.CompletedTask;
    }
}
