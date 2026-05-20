namespace Crestacle.Sentinel.Core.Events;

/// <summary>
/// Published after a pending dual-approval assignment is approved and the role is granted.
/// <see cref="RoleExpiresOn"/> carries the time-bound constraint from the original request,
/// or <c>null</c> for a permanent assignment.
/// </summary>
public sealed record AssignmentApprovedEvent(
    Guid      PendingAssignmentId,
    Guid      UserId,
    string?   IdentityId,
    Guid      RoleId,
    string?   TenantId,
    string    Reviewer,
    DateTime  ApprovedAt,
    DateTime? RoleExpiresOn);
