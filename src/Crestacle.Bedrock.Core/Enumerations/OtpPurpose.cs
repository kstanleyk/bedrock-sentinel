namespace Crestacle.Bedrock.Core.Enumerations;

/// <summary>Distinguishes what an OTP code was issued for, preventing cross-purpose replay.</summary>
public enum OtpPurpose
{
    /// <summary>2FA code used to complete a first-factor login.</summary>
    Login,

    /// <summary>Code used to confirm a password reset.</summary>
    PasswordReset,

    /// <summary>Code used to confirm an email address.</summary>
    EmailVerification,

    /// <summary>Code used for step-up re-authentication.</summary>
    StepUp
}
