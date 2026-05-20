namespace Crestacle.Sentinel.Core.Authorization;

/// <summary>
/// Built-in role name constants shared across all applications.
/// Apps define their own additional role constants in their own codebase.
/// </summary>
public static class AppRoles
{
    public const string Admin = nameof(Admin);
    public const string Basic = nameof(Basic);

    private static readonly HashSet<string> DefaultRoles = [Admin, Basic];

    public static bool IsDefault(string role) => DefaultRoles.Contains(role);
}
