namespace Crestacle.Sentinel.Core.DTOs;

public sealed record PendingAssignmentDto(
    Guid      Id,
    Guid      UserId,
    Guid      RoleId,
    string    RequestedBy,
    DateTime  RequestedOn,
    DateTime  ExpiresOn,
    string    Status,
    string?   ReviewedBy,
    DateTime? ReviewedOn,
    string?   RejectionReason,
    DateTime? RoleExpiresOn);
