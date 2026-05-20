using Crestacle.Bedrock.Core.Entities;
using Crestacle.Bedrock.Core.Enumerations;

namespace Crestacle.Bedrock.Core.DTOs;

/// <summary>Filter parameters for the audit log query endpoint.</summary>
public sealed record AuditQueryFilter(
    Guid? UserId = null,
    AuditEventType? EventType = null,
    DateTime? From = null,
    DateTime? To = null,
    int Page = 1,
    int PageSize = 50);

/// <summary>Paginated result set from an audit log query.</summary>
public sealed record AuditQueryResult(
    IReadOnlyList<AuditEntry> Items,
    int TotalCount);
