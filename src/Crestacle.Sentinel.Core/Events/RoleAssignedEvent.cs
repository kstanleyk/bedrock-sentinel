namespace Crestacle.Sentinel.Core.Events;

/// <summary>
/// Published after a role is directly assigned to a user (not via dual-approval flow).
/// </summary>
public sealed record RoleAssignedEvent(
    Guid      UserId,
    string?   IdentityId,
    Guid      RoleId,
    string?   TenantId,
    DateTime? ExpiresOn,
    DateTime  AssignedAt);
