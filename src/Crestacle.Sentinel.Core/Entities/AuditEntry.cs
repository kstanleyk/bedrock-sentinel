using Crestacle.Sentinel.Core.Enums;

namespace Crestacle.Sentinel.Core.Entities;

/// <summary>
/// Immutable record of every permission or role change made through Sentinel.
/// Rows are append-only — never update or delete them.
/// </summary>
public sealed class AuditEntry : Entity<Guid>
{
    /// <summary>What happened (role assigned/removed, permission assigned/removed).</summary>
    public AuditAction Action          { get; private set; }

    /// <summary>Slash-separated composite key describing the affected record, e.g. "userId/roleId".</summary>
    public string EntityId             { get; private set; } = string.Empty;

    /// <summary>IdentityId of the user who triggered the change. "system" for seeded or background changes.</summary>
    public string ActorIdentityId      { get; private set; } = string.Empty;

    /// <summary>Remote IP address of the actor at the time of the change.</summary>
    public string? ActorIp             { get; private set; }

    /// <summary>User-Agent header of the actor's client.</summary>
    public string? ActorUserAgent      { get; private set; }

    public DateTime CreatedOn          { get; private set; }

    private AuditEntry() { }

    public static AuditEntry Create(
        AuditAction action,
        string      entityId,
        string?     actorIdentityId,
        string?     actorIp,
        string?     actorUserAgent)
        => new()
        {
            Id              = Guid.NewGuid(),
            Action          = action,
            EntityId        = entityId,
            ActorIdentityId = actorIdentityId ?? "system",
            ActorIp         = actorIp,
            ActorUserAgent  = actorUserAgent,
            CreatedOn       = DateTime.UtcNow,
        };
}
