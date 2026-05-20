using Crestacle.Bedrock.Core.Entities;

namespace Crestacle.Bedrock.Core.Interfaces.Repositories;

/// <summary>Persistence contract for <see cref="StepUpChallenge"/>.</summary>
public interface IStepUpChallengeRepository
{
    /// <summary>Returns the step-up challenge with the given surrogate <paramref name="id"/>, or <see langword="null"/> if not found.</summary>
    Task<StepUpChallenge?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Persists a newly created step-up challenge.</summary>
    Task AddAsync(StepUpChallenge challenge, CancellationToken ct = default);

    /// <summary>Persists state changes to an existing step-up challenge (e.g., marking it used).</summary>
    Task UpdateAsync(StepUpChallenge challenge, CancellationToken ct = default);

    /// <summary>Deletes all step-up challenges whose expiry has passed; intended for periodic background cleanup.</summary>
    Task ExpireStaleAsync(CancellationToken ct = default);
}
