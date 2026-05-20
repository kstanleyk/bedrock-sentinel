using Crestacle.Bedrock.Core.Entities;

namespace Crestacle.Bedrock.Core.Interfaces.Repositories;

/// <summary>
/// Append-only persistence contract for <see cref="ConsentRecord"/>.
/// Intentionally exposes no update or delete operations.
/// </summary>
public interface IConsentRepository
{
    /// <summary>Persists a new consent record.</summary>
    Task AddAsync(ConsentRecord record, CancellationToken ct = default);

    /// <summary>Returns all consent records for <paramref name="userId"/>, ordered chronologically.</summary>
    Task<IReadOnlyList<ConsentRecord>> GetForUserAsync(Guid userId, CancellationToken ct = default);
}
