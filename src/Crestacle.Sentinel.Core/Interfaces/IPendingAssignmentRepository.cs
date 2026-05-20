using Crestacle.Sentinel.Core.DTOs;
using Crestacle.Sentinel.Core.Entities;

namespace Crestacle.Sentinel.Core.Interfaces;

public interface IPendingAssignmentRepository
{
    Task<PagedResult<PendingAssignmentDto>> GetPendingAsync(int page = 1, int pageSize = 50, CancellationToken ct = default);
    Task<PendingAssignment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PendingAssignment> CreateAsync(Guid userId, Guid roleId, DateTime? roleExpiresOn = null, CancellationToken ct = default);

    /// <summary>
    /// Approves the pending assignment. Creates the UserRole row and writes an audit entry.
    /// Returns false when the request no longer exists, is already reviewed, or has expired.
    /// </summary>
    Task<bool> ApproveAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Rejects the pending assignment.
    /// Returns false when the request no longer exists, is already reviewed, or has expired.
    /// </summary>
    Task<bool> RejectAsync(Guid id, string? reason, CancellationToken ct = default);

    /// <summary>
    /// Marks all Pending assignments whose ExpiresOn is in the past as Expired.
    /// Called by the background expiry sweep.
    /// </summary>
    Task MarkExpiredBatchAsync(CancellationToken ct = default);
}
