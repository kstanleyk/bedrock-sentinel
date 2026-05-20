using Crestacle.Bedrock.Core.DTOs;

namespace Crestacle.Bedrock.Core.Interfaces.Services;

/// <summary>
/// Validates an external provider token and returns verified identity claims.
/// Implement this interface for each social/OAuth provider (e.g. Google, GitHub).
/// Register implementations via <c>builder.WithExternalIdentityValidator&lt;T&gt;()</c>.
/// </summary>
public interface IExternalIdentityValidator
{
    /// <summary>The provider name this validator handles (e.g. "google", "github").</summary>
    string ProviderName { get; }

    /// <summary>
    /// Validates the provider token and returns verified identity claims, or <c>null</c>
    /// if validation fails.
    /// </summary>
    /// <param name="providerToken">The raw provider token to validate.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Verified <see cref="ExternalIdentityClaims"/> on success, or <c>null</c> when the token is invalid or expired.</returns>
    Task<ExternalIdentityClaims?> ValidateAsync(string providerToken, CancellationToken ct = default);
}
