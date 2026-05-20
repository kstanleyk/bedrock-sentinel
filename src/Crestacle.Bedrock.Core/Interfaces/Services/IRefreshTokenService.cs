using Crestacle.Bedrock.Core.DTOs;
using Crestacle.Bedrock.Core.Exceptions;

namespace Crestacle.Bedrock.Core.Interfaces.Services;

/// <summary>Orchestrates refresh token issuance, rotation, and revocation.</summary>
public interface IRefreshTokenService
{
    /// <summary>
    /// Issues a fresh <see cref="TokenPair"/> after a successful login.
    /// Creates a new <see cref="Core.Entities.RefreshToken"/> and <see cref="Core.Entities.Session"/>.
    /// Evicts the oldest session when the per-user concurrent session limit is reached.
    /// </summary>
    /// <param name="userId">The authenticated user's ID.</param>
    /// <param name="email">The user's email address, embedded in the access token.</param>
    /// <param name="roles">Role names to embed as claims in the access token.</param>
    /// <param name="ip">The client IP address for audit logging and anomaly detection.</param>
    /// <param name="userAgent">The client User-Agent header for device fingerprinting.</param>
    /// <param name="fingerprintHash">Pre-computed device fingerprint hash.</param>
    /// <param name="tenantId">Optional tenant identifier to embed in the access token.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="TokenPair"/> containing the new access and refresh tokens.</returns>
    Task<TokenPair> IssueAsync(
        Guid userId,
        string email,
        IEnumerable<string> roles,
        string ip,
        string userAgent,
        string fingerprintHash,
        string? tenantId = null,
        CancellationToken ct = default);

    /// <summary>
    /// Rotates the refresh token: revokes the supplied token, issues a new one,
    /// and updates the associated <see cref="Core.Entities.Session"/>.
    /// </summary>
    /// <param name="rawRefreshToken">The plaintext refresh token to rotate.</param>
    /// <param name="ip">The client IP address for audit logging.</param>
    /// <param name="userAgent">The client User-Agent header.</param>
    /// <param name="fingerprintHash">Pre-computed device fingerprint hash.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A new <see cref="TokenPair"/> with rotated access and refresh tokens.</returns>
    /// <exception cref="BedrockValidationException">Thrown when the token is unknown, already revoked, or expired.</exception>
    Task<TokenPair> RefreshAsync(
        string rawRefreshToken,
        string ip,
        string userAgent,
        string fingerprintHash,
        CancellationToken ct = default);

    /// <summary>
    /// Revokes the given refresh token and its session.
    /// Always returns without error even if the token is unknown or already revoked.
    /// When <paramref name="accessTokenJti"/> is supplied the corresponding access token
    /// is blacklisted in <see cref="IBedrockCache"/> for the remainder of its lifetime.
    /// </summary>
    /// <param name="rawRefreshToken">The plaintext refresh token to revoke.</param>
    /// <param name="ip">The client IP address for audit logging.</param>
    /// <param name="accessTokenJti">Optional JTI of the access token to blacklist in the revocation cache.</param>
    /// <param name="accessTokenRemainingLifetime">Remaining lifetime of the access token; required when <paramref name="accessTokenJti"/> is supplied.</param>
    /// <param name="ct">Cancellation token.</param>
    Task RevokeAsync(
        string rawRefreshToken,
        string ip,
        string? accessTokenJti = null,
        TimeSpan? accessTokenRemainingLifetime = null,
        CancellationToken ct = default);

    /// <summary>Revokes all active refresh tokens and sessions for <paramref name="userId"/>.</summary>
    /// <param name="userId">The user whose tokens and sessions to revoke.</param>
    /// <param name="ip">The client IP address for audit logging.</param>
    /// <param name="ct">Cancellation token.</param>
    Task RevokeAllAsync(Guid userId, string ip, CancellationToken ct = default);
}
