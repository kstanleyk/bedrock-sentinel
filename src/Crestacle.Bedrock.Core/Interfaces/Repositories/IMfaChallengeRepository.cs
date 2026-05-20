using Crestacle.Bedrock.Core.Entities;

namespace Crestacle.Bedrock.Core.Interfaces.Repositories;

/// <summary>Persistence contract for <see cref="MfaChallenge"/>.</summary>
public interface IMfaChallengeRepository
{
    /// <summary>Returns the MFA challenge with the given surrogate <paramref name="id"/>, or <see langword="null"/> if not found.</summary>
    Task<MfaChallenge?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Persists a newly created MFA challenge.</summary>
    Task AddAsync(MfaChallenge challenge, CancellationToken ct = default);

    /// <summary>Persists state changes to an existing MFA challenge (e.g., marking it used).</summary>
    Task UpdateAsync(MfaChallenge challenge, CancellationToken ct = default);

    /// <summary>Deletes all MFA challenges whose expiry has passed; intended for periodic background cleanup.</summary>
    Task ExpireStaleAsync(CancellationToken ct = default);
}
