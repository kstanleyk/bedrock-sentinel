using Crestacle.Sentinel.Core.Authorization;

namespace Crestacle.Sentinel.Core.Entities;

public sealed class Permission : Entity<string>
{
    public string  Feature     { get; private set; } = string.Empty;
    public string  Action      { get; private set; } = string.Empty;
    public string  Group       { get; private set; } = string.Empty;
    public string  Description { get; private set; } = string.Empty;
    public bool    IsBasic     { get; private set; }
    public DateTime CreatedOn  { get; private set; }

    /// <summary>Optimistic concurrency token — set by the database (nullable for PostgreSQL).</summary>
    public byte[]? RowVersion  { get; private set; }

    public ICollection<RolePermission> RolePermissions { get; private set; } = [];

    private Permission() { }

    public static Permission Create(AppPermission appPermission)
        => new()
        {
            Id          = AppPermission.NameFor(appPermission.Feature, appPermission.Action),
            Feature     = appPermission.Feature,
            Action      = appPermission.Action,
            Group       = appPermission.Group,
            Description = appPermission.Description,
            IsBasic     = appPermission.IsBasic,
            CreatedOn   = DateTime.UtcNow,
        };
}
