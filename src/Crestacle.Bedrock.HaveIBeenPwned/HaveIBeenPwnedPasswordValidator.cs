using System.Security.Cryptography;
using System.Text;
using Crestacle.Bedrock.Core.Interfaces.Services;

namespace Crestacle.Bedrock.HaveIBeenPwned;

/// <summary>
/// Decorates an inner <see cref="IPasswordValidator"/> with a Have I Been Pwned k-anonymity
/// breach check. Complexity rules and history checks are delegated to the inner validator.
/// The HIBP API call is fail-open: a network failure never blocks registration.
/// </summary>
public sealed class HaveIBeenPwnedPasswordValidator : IPasswordValidator
{
    private const string BreachMessage =
        "This password has appeared in a known data breach. Please choose a different password.";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IPasswordValidator _inner;

    public HaveIBeenPwnedPasswordValidator(IHttpClientFactory httpClientFactory, IPasswordValidator inner)
    {
        _httpClientFactory = httpClientFactory;
        _inner = inner;
    }

    /// <inheritdoc/>
    public bool IsValid(string password, out IReadOnlyList<string> errors)
    {
        if (!_inner.IsValid(password, out errors))
            return false;

        try
        {
            var breached = Task.Run(() => IsBreachedAsync(password)).GetAwaiter().GetResult();
            if (breached)
            {
                errors = [BreachMessage];
                return false;
            }
        }
        catch
        {
            // fail-open: any exception (network, timeout, parse) → let the password through
        }

        return true;
    }

    /// <inheritdoc/>
    public bool IsPreviouslyUsed(string password, IEnumerable<string> hashes)
        => _inner.IsPreviouslyUsed(password, hashes);

    private async Task<bool> IsBreachedAsync(string password)
    {
        var hash = ComputeSha1Hex(password);
        var prefix = hash[..5];
        var suffix = hash[5..];

        var client = _httpClientFactory.CreateClient("hibp");
        var body = await client.GetStringAsync($"/range/{prefix}").ConfigureAwait(false);

        // Each line is "<35-char-suffix>:<count>". A case-insensitive search for the suffix
        // string within the body is sufficient: the 35-char hex suffix won't collide with counts.
        return body.Contains(suffix, StringComparison.OrdinalIgnoreCase);
    }

    private static string ComputeSha1Hex(string password)
    {
        var bytes = SHA1.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes); // uppercase hex
    }
}
