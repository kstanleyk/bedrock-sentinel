namespace Crestacle.Bedrock.Core.Enumerations;

/// <summary>Second-factor verification method configured for a user credential.</summary>
public enum MfaMethod
{
    /// <summary>RFC 6238 time-based OTP verified via an authenticator app.</summary>
    Totp,

    /// <summary>One-time code delivered via email.</summary>
    EmailOtp,

    /// <summary>One-time code delivered via SMS.</summary>
    SmsOtp
}
