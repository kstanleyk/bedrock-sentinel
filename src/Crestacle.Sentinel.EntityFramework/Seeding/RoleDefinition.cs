using Crestacle.Sentinel.Core.Enums;

namespace Crestacle.Sentinel.EntityFramework.Seeding;

/// <summary>Describes a role and the permissions to assign to it during seeding.</summary>
public sealed record RoleDefinition(
    string Name,
    string DisplayName,
    RoleType Type,
    IEnumerable<string> PermissionIds);
