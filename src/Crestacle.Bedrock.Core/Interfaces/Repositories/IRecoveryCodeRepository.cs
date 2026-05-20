using Crestacle.Bedrock.Core.Entities;

namespace Crestacle.Bedrock.Core.Interfaces.Repositories;

/// <summary>Persistence contract for <see cref="RecoveryCode"/>.</summary>
public interface IRecoveryCodeRepository
{
    /// <summary>Returns all unused recovery codes belonging to <paramref name="userId"/>.</summary>
    Task<IReadOnlyList<RecoveryCode>> GetAvailableByUserAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Returns the recovery code owned by <paramref name="userId"/> whose hash matches <paramref name="codeHash"/>, or <see langword="null"/> if not found.</summary>
    Task<RecoveryCode?> GetByHashAsync(Guid userId, string codeHash, CancellationToken ct = default);

    /// <summary>Persists a batch of newly generated recovery codes in a single operation.</summary>
    Task AddRangeAsync(IEnumerable<RecoveryCode> codes, CancellationToken ct = default);

    /// <summary>Persists state changes to an existing recovery code (e.g., marking it consumed).</summary>
    Task UpdateAsync(RecoveryCode code, CancellationToken ct = default);

    /// <summary>Marks all recovery codes for <paramref name="userId"/> as consumed; called when codes are regenerated.</summary>
    Task InvalidateAllForUserAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Returns the count of unused recovery codes remaining for <paramref name="userId"/>.</summary>
    Task<int> CountAvailableAsync(Guid userId, CancellationToken ct = default);
}
