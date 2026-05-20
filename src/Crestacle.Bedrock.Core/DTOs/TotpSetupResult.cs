namespace Crestacle.Bedrock.Core.DTOs;

/// <summary>
/// Returned when TOTP setup is initiated. The <see cref="QrUri"/> encodes the TOTP secret
/// in <c>otpauth://totp/</c> format; the user scans this with an authenticator app.
/// A confirmation code must be submitted to activate TOTP.
/// </summary>
public sealed record TotpSetupResult(string QrUri);
