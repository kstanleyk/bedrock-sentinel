using Crestacle.Sentinel.Core.Authorization;
using Crestacle.Sentinel.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Crestacle.Sentinel.EntityFramework.Seeding;

/// <summary>
/// Base seeder that populates the Sentinel auth tables.
/// Subclass this in each app and implement GetPermissions() and GetRoles().
/// </summary>
public abstract class SentinelSeeder(IAuthDbContext context)
{
    /// <summary>Returns all permission definitions for this application.</summary>
    protected abstract IEnumerable<AppPermission> GetPermissions();

    /// <summary>Returns all role definitions with their assigned permission IDs.</summary>
    protected abstract IEnumerable<RoleDefinition> GetRoles();

    /// <summary>
    /// Returns the permissions required by Sentinel's own management endpoints.
    /// These are seeded automatically so callers never get a silent 403.
    /// Override to suppress or extend; call <c>base.GetBuiltInPermissions()</c> to keep the defaults.
    /// </summary>
    protected virtual IEnumerable<AppPermission> GetBuiltInPermissions() =>
    [
        new AppPermission(SentinelFeature.User,  AppAction.Read,   "Sentinel", "View users and their role assignments",  isBasic: true),
        new AppPermission(SentinelFeature.User,  AppAction.Update, "Sentinel", "Assign and remove roles from users"),
        new AppPermission(SentinelFeature.Role,  AppAction.Read,   "Sentinel", "View roles and their permission sets",   isBasic: true),
        new AppPermission(SentinelFeature.Role,  AppAction.Update, "Sentinel", "Add and remove permissions from roles"),
        new AppPermission(SentinelFeature.Audit, AppAction.Read,   "Sentinel", "Read the immutable audit log"),
    ];

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await SeedPermissionsAsync(ct);
        await SeedRolesAsync(ct);
    }

    private async Task SeedPermissionsAsync(CancellationToken ct)
    {
        // Seed built-in Sentinel management permissions first, then app-specific ones.
        var all = GetBuiltInPermissions().Concat(GetPermissions());

        foreach (var appPerm in all)
        {
            var id = AppPermission.NameFor(appPerm.Feature, appPerm.Action);
            if (!await context.Permissions.AnyAsync(p => p.Id == id, ct))
                context.Permissions.Add(Permission.Create(appPerm));
        }

        await context.SaveChangesAsync(ct);
    }

    private async Task SeedRolesAsync(CancellationToken ct)
    {
        foreach (var roleDef in GetRoles())
        {
            if (await context.Roles.AnyAsync(r => r.Name == roleDef.Name, ct))
                continue;

            var role = Role.Create(roleDef.Name, roleDef.DisplayName, roleDef.Type);
            context.Roles.Add(role);
            await context.SaveChangesAsync(ct);

            foreach (var permId in roleDef.PermissionIds)
            {
                if (await context.Permissions.AnyAsync(p => p.Id == permId, ct))
                {
                    context.RolePermissions.Add(new RolePermission
                    {
                        RoleId       = role.Id,
                        PermissionId = permId,
                        CreatedOn    = DateTime.UtcNow
                    });
                }
            }

            await context.SaveChangesAsync(ct);
        }
    }
}
