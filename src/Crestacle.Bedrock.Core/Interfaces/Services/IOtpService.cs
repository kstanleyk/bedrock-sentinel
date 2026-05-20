namespace Crestacle.Bedrock.Core.Interfaces.Services;

/// <summary>One-time password generation and verification for email and SMS delivery.</summary>
public interface IOtpService
{
    /// <summary>Generates a cryptographically random 6-digit code, zero-padded.</summary>
    /// <returns>A zero-padded 6-digit numeric string (e.g. <c>"042819"</c>).</returns>
    string GenerateCode();

    /// <summary>Returns the SHA-256 hex digest of a plaintext OTP code.</summary>
    /// <param name="rawCode">The plaintext OTP code to hash.</param>
    /// <returns>A lowercase hexadecimal SHA-256 digest.</returns>
    string HashCode(string rawCode);

    /// <summary>
    /// Returns <c>true</c> when the SHA-256 of <paramref name="rawCode"/> matches <paramref name="hash"/>
    /// using constant-time comparison.
    /// </summary>
    /// <param name="rawCode">The plaintext OTP code supplied by the user.</param>
    /// <param name="hash">The stored SHA-256 hex hash of the original code.</param>
    /// <returns><c>true</c> when the code matches the hash; <c>false</c> otherwise.</returns>
    bool VerifyCode(string rawCode, string hash);
}
