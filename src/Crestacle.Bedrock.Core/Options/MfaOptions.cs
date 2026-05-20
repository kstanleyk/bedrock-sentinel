namespace Crestacle.Bedrock.Core.Options;

/// <summary>Multi-factor authentication policy settings.</summary>
public sealed class MfaOptions
{
    /// <summary>Issuer name displayed in TOTP authenticator apps.</summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Days before mandatory MFA enforcement begins for users in <see cref="MandatoryRoles"/>.
    /// Default: 14 days.
    /// </summary>
    public int GracePeriodDays { get; set; } = 14;

    /// <summary>
    /// Roles that must have MFA enrolled. Users in these roles will receive a grace period
    /// and then be blocked from business endpoints until they enroll.
    /// </summary>
    public IList<string> MandatoryRoles { get; set; } = new List<string>();

    /// <summary>Number of recovery codes to generate per MFA setup. Default: 10.</summary>
    public int BackupCodeCount { get; set; } = 10;
}
