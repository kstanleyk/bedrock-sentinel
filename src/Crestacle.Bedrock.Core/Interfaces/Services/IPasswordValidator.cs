namespace Crestacle.Bedrock.Core.Interfaces.Services;

/// <summary>Validates password complexity and enforces the reuse-prevention policy.</summary>
public interface IPasswordValidator
{
    /// <summary>
    /// Returns <c>true</c> when <paramref name="password"/> satisfies all configured complexity rules.
    /// Populates <paramref name="errors"/> with a human-readable description of each violation.
    /// </summary>
    /// <param name="password">The plaintext password to validate.</param>
    /// <param name="errors">Output list of violation messages; empty when the password is valid.</param>
    /// <returns><c>true</c> when the password passes all rules; <c>false</c> when one or more rules are violated.</returns>
    bool IsValid(string password, out IReadOnlyList<string> errors);

    /// <summary>
    /// Returns <c>true</c> when <paramref name="password"/> matches any of the previously stored hashes,
    /// indicating that reuse is not permitted. Uses constant-time comparison internally.
    /// </summary>
    /// <param name="password">The plaintext password to check for reuse.</param>
    /// <param name="hashes">The collection of previously stored password hashes to compare against.</param>
    /// <returns><c>true</c> when the password has been used before; <c>false</c> when it is new.</returns>
    bool IsPreviouslyUsed(string password, IEnumerable<string> hashes);
}
