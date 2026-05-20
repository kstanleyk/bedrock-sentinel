namespace Crestacle.Bedrock.Core.Interfaces.Services;

/// <summary>
/// Password hashing contract. The default implementation uses Argon2id and
/// transparently detects legacy PBKDF2 hashes for migration.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>Returns an Argon2id hash of <paramref name="password"/>.</summary>
    /// <param name="password">The plaintext password to hash.</param>
    /// <returns>An Argon2id hash string in the format <c>argon2id${base64(salt)}${base64(hash)}</c>.</returns>
    string Hash(string password);

    /// <summary>Returns <c>true</c> when <paramref name="password"/> matches <paramref name="hash"/>.</summary>
    /// <param name="password">The plaintext password to verify.</param>
    /// <param name="hash">The stored hash to verify against (Argon2id or legacy PBKDF2).</param>
    /// <returns><c>true</c> when the password matches the hash; <c>false</c> otherwise.</returns>
    bool Verify(string password, string hash);

    /// <summary>
    /// Returns <c>true</c> when the hash was not produced by the current algorithm
    /// (e.g. a legacy PBKDF2 hash), indicating that it should be rehashed on the next successful login.
    /// </summary>
    /// <param name="hash">The stored hash string to inspect.</param>
    /// <returns><c>true</c> when the hash format is outdated; <c>false</c> when it is current.</returns>
    bool NeedsRehash(string hash);
}
