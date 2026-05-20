namespace Crestacle.Sentinel.Core.Authorization;

/// <summary>Describes a single permission used to populate the database and seed roles.</summary>
public sealed class AppPermission(
    string feature,
    string action,
    string group,
    string description,
    bool isBasic = false)
{
    public string Feature     { get; } = feature;
    public string Action      { get; } = action;
    public string Group       { get; } = group;
    public string Description { get; } = description;
    public bool   IsBasic     { get; } = isBasic;

    /// <summary>Returns the canonical policy/permission name: "Permission.{Feature}.{Action}".</summary>
    public static string NameFor(string feature, string action)
        => $"{AppClaim.Permission}.{feature}.{action}";

    public string Name => NameFor(Feature, Action);
}
