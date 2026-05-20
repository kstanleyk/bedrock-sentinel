using Crestacle.Bedrock.Core.DTOs;
using Crestacle.Bedrock.Core.Entities;

namespace Crestacle.Bedrock.Core.Interfaces.Repositories;

/// <summary>
/// Append-only persistence contract for <see cref="AuditEntry"/>.
/// Intentionally exposes no update or delete operations.
/// </summary>
public interface IAuditRepository
{
    /// <summary>Appends a new audit entry to the log.</summary>
    Task AddAsync(AuditEntry entry, CancellationToken ct = default);

    /// <summary>
    /// Returns a paginated, filtered slice of the audit log.
    /// All filter parameters are optional and combined with AND semantics.
    /// </summary>
    Task<AuditQueryResult> QueryAsync(AuditQueryFilter filter, CancellationToken ct = default);
}
