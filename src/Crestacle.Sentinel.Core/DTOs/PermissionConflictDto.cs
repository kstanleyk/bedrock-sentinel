namespace Crestacle.Sentinel.Core.DTOs;

public sealed record PermissionConflictDto(
    Guid     Id,
    string   PermissionIdA,
    string   PermissionIdB,
    string   CreatedBy,
    DateTime CreatedOn);
