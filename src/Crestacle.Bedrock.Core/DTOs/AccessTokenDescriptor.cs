namespace Crestacle.Bedrock.Core.DTOs;

/// <summary>
/// Describes an issued access token: its signed string, unique identifier, and expiry time.
/// Returned by <see cref="Interfaces.Services.IBedrockTokenIssuer"/> so that the session layer
/// can track JTI-based revocation regardless of who signed the token.
/// </summary>
/// <param name="AccessToken">The signed access token string (JWT or any format the issuer produces).</param>
/// <param name="Jti">
/// The token's unique identifier, stored against the session so that
/// <see cref="Interfaces.Services.IRefreshTokenService.RevokeAllAsync"/> can blacklist it.
/// For JWTs this is the <c>jti</c> claim; for opaque tokens supply any stable unique value.
/// </param>
/// <param name="ExpiresAt">UTC time at which the access token expires.</param>
public sealed record AccessTokenDescriptor(
    string AccessToken,
    string Jti,
    DateTime ExpiresAt);
