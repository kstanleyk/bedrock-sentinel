namespace Crestacle.Sentinel.Core.Authorization;

/// <summary>
/// Standard CRUD actions available to every application.
/// Apps may define additional workflow-specific actions in their own codebase.
/// </summary>
public static class AppAction
{
    public const string Create = nameof(Create);
    public const string Read   = nameof(Read);
    public const string Update = nameof(Update);
    public const string Delete = nameof(Delete);
}
