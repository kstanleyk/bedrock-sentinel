using Crestacle.Bedrock.Core.Entities;

namespace Crestacle.Bedrock.Core.Interfaces.Repositories;

/// <summary>Persistence contract for <see cref="RefreshToken"/>.</summary>
public interface IRefreshTokenRepository
{
    /// <summary>Returns the token whose SHA-256 hash matches <paramref name="tokenHash"/>, or <see langword="null"/> if not found.</summary>
    Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken ct = default);

    /// <summary>Persists a newly issued refresh token.</summary>
    Task AddAsync(RefreshToken token, CancellationToken ct = default);

    /// <summary>Persists state changes to an existing refresh token (e.g., rotation or revocation).</summary>
    Task UpdateAsync(RefreshToken token, CancellationToken ct = default);

    /// <summary>Revokes all active refresh tokens belonging to <paramref name="userId"/>, recording <paramref name="byIp"/> as the revocation origin.</summary>
    Task RevokeAllForUserAsync(Guid userId, string byIp, CancellationToken ct = default);

    /// <summary>Returns the number of non-revoked, non-expired refresh tokens currently held by <paramref name="userId"/>.</summary>
    Task<int> CountActiveForUserAsync(Guid userId, CancellationToken ct = default);
}
