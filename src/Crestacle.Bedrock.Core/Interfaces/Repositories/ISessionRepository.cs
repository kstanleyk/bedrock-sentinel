using Crestacle.Bedrock.Core.Entities;

namespace Crestacle.Bedrock.Core.Interfaces.Repositories;

/// <summary>Persistence contract for <see cref="Session"/>.</summary>
public interface ISessionRepository
{
    /// <summary>Returns the session with the given surrogate <paramref name="id"/>, or <see langword="null"/> if not found.</summary>
    Task<Session?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Returns the session whose opaque-token hash matches <paramref name="tokenHash"/>, or <see langword="null"/> if not found.</summary>
    Task<Session?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default);

    /// <summary>Returns all non-revoked sessions belonging to <paramref name="userId"/>.</summary>
    Task<IReadOnlyList<Session>> GetActiveByUserAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Persists a newly created session.</summary>
    Task AddAsync(Session session, CancellationToken ct = default);

    /// <summary>Persists state changes to an existing session (e.g., refresh or revocation).</summary>
    Task UpdateAsync(Session session, CancellationToken ct = default);

    /// <summary>Revokes all active sessions belonging to <paramref name="userId"/>, recording <paramref name="byIp"/> as the revocation origin.</summary>
    Task RevokeAllForUserAsync(Guid userId, string byIp, CancellationToken ct = default);

    /// <summary>Returns the count of non-revoked sessions currently held by <paramref name="userId"/>.</summary>
    Task<int> CountActiveForUserAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Returns the least-recently-created active session for <paramref name="userId"/>, used to evict the oldest session when the concurrent limit is reached; returns <see langword="null"/> if the user has no active sessions.</summary>
    Task<Session?> GetOldestActiveForUserAsync(Guid userId, CancellationToken ct = default);
}
