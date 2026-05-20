using System.Security.Cryptography;
using System.Text;
using Crestacle.Bedrock.Core.Interfaces.Services;
using Konscious.Security.Cryptography;
using Microsoft.AspNetCore.Identity;

namespace Crestacle.Bedrock.AspNetCore.Services;

/// <summary>
/// Argon2id password hasher. Hash format: <c>argon2id${base64(salt)}${base64(hash)}</c>.
/// Transparently detects and verifies legacy ASP.NET Core Identity PBKDF2 hashes
/// (v2 and v3) and BCrypt hashes (<c>$2*</c>) so they can be migrated to Argon2id
/// on next successful login.
/// </summary>
public sealed class Argon2idPasswordHasher : IPasswordHasher
{
    private const int MemorySize = 65536; // 64 MB
    private const int Iterations = 3;
    private const int Parallelism = 4;
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const string Prefix = "argon2id$";
    private const string BcryptPrefix = "$2";

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Compute(Encoding.UTF8.GetBytes(password), salt);
        return $"{Prefix}{Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string hash)
    {
        if (hash.StartsWith(Prefix, StringComparison.Ordinal))
            return VerifyArgon2id(password, hash);

        if (hash.StartsWith(BcryptPrefix, StringComparison.Ordinal))
        {
            try { return BCrypt.Net.BCrypt.Verify(password, hash); }
            catch { return false; }
        }

        try
        {
            var result = new PasswordHasher<object>().VerifyHashedPassword(null!, hash, password);
            return result != PasswordVerificationResult.Failed;
        }
        catch
        {
            return false;
        }
    }

    public bool NeedsRehash(string hash)
        => !hash.StartsWith(Prefix, StringComparison.Ordinal);

    private bool VerifyArgon2id(string password, string hash)
    {
        var body = hash[Prefix.Length..];
        var sep = body.IndexOf('$');
        if (sep < 0) return false;

        byte[] salt, expected;
        try
        {
            salt = Convert.FromBase64String(body[..sep]);
            expected = Convert.FromBase64String(body[(sep + 1)..]);
        }
        catch { return false; }

        var actual = Compute(Encoding.UTF8.GetBytes(password), salt);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }

    private static byte[] Compute(byte[] passwordBytes, byte[] salt)
    {
        using var argon2 = new Argon2id(passwordBytes)
        {
            Salt = salt,
            MemorySize = MemorySize,
            Iterations = Iterations,
            DegreeOfParallelism = Parallelism
        };
        return argon2.GetBytes(HashSize);
    }
}
