namespace Crestacle.Bedrock.Core.Options;

/// <summary>Password complexity policy and history settings.</summary>
public sealed class PasswordOptions
{
    /// <summary>Minimum password length. Default: 12.</summary>
    public int MinLength { get; set; } = 12;

    /// <summary>Require at least one uppercase letter. Default: true.</summary>
    public bool RequireUppercase { get; set; } = true;

    /// <summary>Require at least one lowercase letter. Default: true.</summary>
    public bool RequireLowercase { get; set; } = true;

    /// <summary>Require at least one digit. Default: true.</summary>
    public bool RequireDigit { get; set; } = true;

    /// <summary>Require at least one special character. Default: true.</summary>
    public bool RequireSpecialCharacter { get; set; } = true;

    /// <summary>
    /// Number of previous hashes to retain for reuse prevention. Default: 5.
    /// Set to 0 to disable history checks.
    /// </summary>
    public int HistoryDepth { get; set; } = 5;

    /// <summary>
    /// Number of days before a password expires. Default: 0 (never expires).
    /// </summary>
    public int ExpiryDays { get; set; } = 0;

    /// <summary>
    /// Path to a deny-list file of common/weak passwords (one per line, UTF-8).
    /// Gzip-compressed files are supported and detected by the <c>.gz</c> extension.
    /// Set to <c>"embedded"</c> to use the built-in top-1000 deny-list.
    /// Leave <c>null</c> (default) to disable the feature.
    /// </summary>
    public string? CommonPasswordDenyListPath { get; set; }
}
