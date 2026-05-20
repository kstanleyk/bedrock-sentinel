namespace Crestacle.Sentinel.Core.DTOs;

public sealed record PermissionDto(
    string Id,
    string Feature,
    string Action,
    string Group,
    string Description);
