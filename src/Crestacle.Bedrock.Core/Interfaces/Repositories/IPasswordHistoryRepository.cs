using Crestacle.Bedrock.Core.Entities;

namespace Crestacle.Bedrock.Core.Interfaces.Repositories;

/// <summary>Persistence contract for <see cref="PasswordHistory"/>.</summary>
public interface IPasswordHistoryRepository
{
    /// <summary>Returns the <paramref name="depth"/> most recent password history entries for <paramref name="userId"/>, newest first.</summary>
    Task<IReadOnlyList<PasswordHistory>> GetRecentByUserAsync(Guid userId, int depth, CancellationToken ct = default);

    /// <summary>Persists a new password history entry.</summary>
    Task AddAsync(PasswordHistory entry, CancellationToken ct = default);

    /// <summary>Deletes history entries beyond the most recent <paramref name="keepCount"/> for <paramref name="userId"/>.</summary>
    Task PruneAsync(Guid userId, int keepCount, CancellationToken ct = default);
}
