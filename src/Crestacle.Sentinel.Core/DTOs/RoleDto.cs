namespace Crestacle.Sentinel.Core.DTOs;

public sealed record RoleDto(
    Guid                       Id,
    string                     Name,
    string                     DisplayName,
    string                     Type,
    bool                       RequiresDualApproval,
    IEnumerable<PermissionDto> Permissions);
