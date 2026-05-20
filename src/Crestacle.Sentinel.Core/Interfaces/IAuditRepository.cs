using Crestacle.Sentinel.Core.DTOs;
using Crestacle.Sentinel.Core.Enums;

namespace Crestacle.Sentinel.Core.Interfaces;

public interface IAuditRepository
{
    /// <summary>
    /// Returns a paginated, filtered view of the immutable audit log.
    /// All filters are optional and can be combined.
    /// </summary>
    Task<PagedResult<AuditEntryDto>> GetAsync(
        string?      actorIdentityId = null,
        AuditAction? action          = null,
        DateTime?    from            = null,
        DateTime?    to              = null,
        int          page            = 1,
        int          pageSize        = 50,
        CancellationToken ct         = default);
}
