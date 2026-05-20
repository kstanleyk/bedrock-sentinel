namespace Crestacle.Bedrock.Core.Interfaces;

/// <summary>
/// Delivers OTP codes via SMS for <c>MfaMethod.SmsOtp</c>. The default implementation is
/// a no-op that logs a warning; consumers must provide a real implementation (e.g. Twilio)
/// for production use.
/// </summary>
public interface ISmsSender
{
    /// <summary>Sends a one-time password code to the user's phone number via SMS.</summary>
    /// <param name="toPhoneNumber">The recipient's phone number in E.164 format (e.g. <c>+14155552671</c>).</param>
    /// <param name="code">The plaintext OTP code to deliver.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SendOtpAsync(string toPhoneNumber, string code, CancellationToken ct = default);
}
