using Crestacle.Bedrock.Core.DTOs;

namespace Crestacle.Bedrock.Core.Interfaces.Services;

/// <summary>
/// Issues access tokens on behalf of Bedrock's session layer.
/// </summary>
/// <remarks>
/// <para>
/// <b>Standalone mode (default)</b>: the built-in <c>JwtBedrockTokenIssuer</c> implementation signs
/// tokens using Bedrock's own key (<c>JwtOptions.SigningKey</c> or <c>SigningCertificate</c>).
/// No action is required.
/// </para>
/// <para>
/// <b>External IDP mode (e.g. OpenIddict)</b>: register a custom implementation before calling
/// <c>AddBedrockAspNetCore()</c> and set <c>options.Jwt.ExternalTokenIssuer = true</c> to prevent
/// Bedrock from also installing its own JWT Bearer authentication scheme:
/// <code>
/// services.AddScoped&lt;IBedrockTokenIssuer, MyOpenIddictTokenIssuer&gt;();
/// services.AddBedrockAspNetCore(opts =>
/// {
///     opts.Jwt.ExternalTokenIssuer = true;
///     // Jwt.SigningKey / SigningCertificate are not required in this mode.
/// });
/// </code>
/// The custom implementation receives the verified user identity and extra claims and is
/// responsible for producing a token string and returning its JTI (or any stable unique ID)
/// and expiry so that Bedrock's session and revocation tracking remain functional.
/// </para>
/// </remarks>
public interface IBedrockTokenIssuer
{
    /// <summary>
    /// Issues an access token for an authenticated user.
    /// </summary>
    /// <param name="userId">The authenticated user's identifier.</param>
    /// <param name="email">The user's email address.</param>
    /// <param name="roles">Role names to embed in the token.</param>
    /// <param name="tenantId">Optional tenant identifier.</param>
    /// <param name="extraClaims">Optional additional claims provided by <see cref="IBedrockClaimsEnricher"/>.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// An <see cref="AccessTokenDescriptor"/> containing the token string, its unique identifier,
    /// and the UTC expiry time.
    /// </returns>
    Task<AccessTokenDescriptor> IssueAccessTokenAsync(
        Guid userId,
        string email,
        IEnumerable<string> roles,
        string? tenantId,
        IDictionary<string, string>? extraClaims,
        CancellationToken ct = default);
}
