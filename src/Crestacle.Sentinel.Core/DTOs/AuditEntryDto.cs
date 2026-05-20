namespace Crestacle.Sentinel.Core.DTOs;

public sealed record AuditEntryDto(
    Guid     Id,
    string   Action,
    string   EntityId,
    string   ActorIdentityId,
    string?  ActorIp,
    string?  ActorUserAgent,
    DateTime CreatedOn);
