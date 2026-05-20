using System.Security.Cryptography.X509Certificates;

namespace Crestacle.Bedrock.Core.Options;

/// <summary>
/// JWT signing and validation settings.
///
/// <b>Key rotation procedure</b> (zero-downtime, grace-period overlap):
/// <list type="number">
///   <item>Generate the new key/certificate. Set it as <see cref="SigningKey"/> (or
///         <see cref="SigningCertificate"/>). Move the old key/certificate to
///         <see cref="PreviousSigningKey"/> (or <see cref="PreviousSigningCertificate"/>).
///         New tokens are signed with the new key; tokens signed with the old key
///         continue to validate.</item>
///   <item>Wait at least one <see cref="AccessTokenExpiry"/> TTL so all tokens issued
///         with the old key have expired naturally.</item>
///   <item>Remove <see cref="PreviousSigningKey"/> / <see cref="PreviousSigningCertificate"/>.
///         The old key is fully retired.</item>
/// </list>
/// </summary>
public sealed class JwtOptions
{
    /// <summary>Token issuer claim value (<c>iss</c>).</summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>Token audience claim value (<c>aud</c>).</summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>HMAC-SHA256 symmetric signing key. Mutually exclusive with <see cref="SigningCertificate"/>.</summary>
    public string? SigningKey { get; set; }

    /// <summary>RSA certificate for RS256 signing. Mutually exclusive with <see cref="SigningKey"/>.</summary>
    public X509Certificate2? SigningCertificate { get; set; }

    /// <summary>
    /// Previous HS256 key retained during a rotation grace period. Tokens signed with this
    /// key continue to validate until they expire. Clear it after one
    /// <see cref="AccessTokenExpiry"/> TTL. Mutually exclusive with
    /// <see cref="PreviousSigningCertificate"/>.
    /// </summary>
    public string? PreviousSigningKey { get; set; }

    /// <summary>
    /// Previous RS256 certificate retained during a rotation grace period. Tokens signed
    /// with this certificate continue to validate until they expire. Clear it after one
    /// <see cref="AccessTokenExpiry"/> TTL. Mutually exclusive with
    /// <see cref="PreviousSigningKey"/>.
    /// </summary>
    public X509Certificate2? PreviousSigningCertificate { get; set; }

    /// <summary>Lifetime of issued access tokens. Default: 15 minutes.</summary>
    public TimeSpan AccessTokenExpiry { get; set; } = TimeSpan.FromMinutes(15);

    /// <summary>Lifetime of issued refresh tokens. Default: 7 days.</summary>
    public TimeSpan RefreshTokenExpiry { get; set; } = TimeSpan.FromDays(7);

    /// <summary>
    /// Acceptable clock skew when validating token lifetimes. Default: zero (strict).
    /// </summary>
    public TimeSpan ClockSkew { get; set; } = TimeSpan.Zero;

    /// <summary>
    /// When <c>true</c>, Bedrock skips installing its own JWT Bearer authentication scheme,
    /// allowing an external IDP (e.g. OpenIddict) to own bearer-token validation.
    /// A custom <see cref="Interfaces.Services.IBedrockTokenIssuer"/> should be registered
    /// to replace the built-in JWT access-token issuance.
    /// </summary>
    /// <remarks>
    /// <b><see cref="SigningKey"/> or <see cref="SigningCertificate"/> is still required</b>
    /// even in this mode. Bedrock always signs step-up tokens, MFA challenge tokens, and
    /// enrollment tokens with its own key — these are internal, single-use tokens that never
    /// pass through the external IDP.
    /// </remarks>
    public bool ExternalTokenIssuer { get; set; }
}
