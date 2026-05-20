namespace Crestacle.Bedrock.Core.Interfaces.Services;

/// <summary>
/// TOTP setup, secret encryption, and recovery code management.
/// </summary>
public interface IMfaService
{
    /// <summary>
    /// Generates a new TOTP secret and the corresponding <c>otpauth://</c> QR URI.
    /// Returns the plaintext Base32 secret (to be encrypted before storage) and the QR URI.
    /// </summary>
    /// <param name="email">The user's email address, embedded in the QR URI label.</param>
    /// <param name="issuer">The application name shown in the authenticator app.</param>
    /// <returns>A tuple of the plaintext Base32 secret and the <c>otpauth://totp/…</c> URI.</returns>
    (string secret, string qrUri) GenerateTotpSetup(string email, string issuer);

    /// <summary>Encrypts a plaintext TOTP secret using ASP.NET Core Data Protection.</summary>
    /// <param name="plainSecret">The plaintext Base32 TOTP secret to encrypt.</param>
    /// <returns>An encrypted, Base64-encoded secret safe to persist.</returns>
    string EncryptSecret(string plainSecret);

    /// <summary>Decrypts an encrypted TOTP secret previously produced by <see cref="EncryptSecret"/>.</summary>
    /// <param name="encryptedSecret">The encrypted Base64-encoded secret.</param>
    /// <returns>The original plaintext Base32 TOTP secret.</returns>
    string DecryptSecret(string encryptedSecret);

    /// <summary>
    /// Verifies a 6-digit TOTP code against the encrypted secret with a ±1 step window.
    /// When <paramref name="userId"/> is supplied the accepted code is recorded in the cache
    /// so that a second call with the same code is rejected as a replay within the 90-second
    /// validity window. Pass <c>null</c> for flows that do not require replay protection
    /// (e.g. TOTP setup confirmation).
    /// </summary>
    /// <param name="encryptedSecret">The encrypted TOTP secret retrieved from storage.</param>
    /// <param name="code">The 6-digit code supplied by the user.</param>
    /// <param name="userId">The user's ID used as the replay-protection cache key; pass <c>null</c> to skip replay protection.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns><c>true</c> when the code is valid and has not been replayed; <c>false</c> otherwise.</returns>
    Task<bool> VerifyTotp(string encryptedSecret, string code, Guid? userId = null, CancellationToken ct = default);

    /// <summary>
    /// Generates <paramref name="count"/> plaintext recovery codes (20-byte random hex strings).
    /// The caller is responsible for hashing and persisting them.
    /// </summary>
    /// <param name="count">The number of recovery codes to generate; defaults to 10.</param>
    /// <returns>A read-only list of plaintext recovery codes to show to the user exactly once.</returns>
    IReadOnlyList<string> GenerateRecoveryCodes(int count = 10);

    /// <summary>
    /// Returns <c>true</c> when <paramref name="code"/> matches <paramref name="hash"/>
    /// using constant-time comparison.
    /// </summary>
    /// <param name="code">The plaintext recovery code supplied by the user.</param>
    /// <param name="hash">The stored SHA-256 hex hash of the original recovery code.</param>
    /// <returns><c>true</c> when the code matches the hash; <c>false</c> otherwise.</returns>
    bool VerifyRecoveryCode(string code, string hash);
}
