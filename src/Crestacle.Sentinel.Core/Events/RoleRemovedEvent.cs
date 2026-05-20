namespace Crestacle.Sentinel.Core.Events;

/// <summary>
/// Published after a role assignment is soft-deleted (removed) from a user.
/// </summary>
public sealed record RoleRemovedEvent(
    Guid     UserId,
    string?  IdentityId,
    Guid     RoleId,
    string?  TenantId,
    DateTime RemovedAt);
