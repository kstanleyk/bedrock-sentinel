using Crestacle.Sentinel.Core.Entities;
using Crestacle.Sentinel.EntityFramework;
using Crestacle.Sentinel.EntityFramework.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Crestacle.Sentinel.Tests.EntityFramework;

/// <summary>Minimal DbContext used only in tests to exercise the Sentinel EF layer.</summary>
internal sealed class TestAuthDbContext(DbContextOptions<TestAuthDbContext> options)
    : DbContext(options), IAuthDbContext
{
    public DbSet<User>               Users               => Set<User>();
    public DbSet<Role>               Roles               => Set<Role>();
    public DbSet<Permission>         Permissions         => Set<Permission>();
    public DbSet<UserRole>           UserRoles           => Set<UserRole>();
    public DbSet<RolePermission>     RolePermissions     => Set<RolePermission>();
    public DbSet<AuditEntry>         AuditLog            => Set<AuditEntry>();
    public DbSet<PermissionConflict> PermissionConflicts => Set<PermissionConflict>();
    public DbSet<PendingAssignment>  PendingAssignments  => Set<PendingAssignment>();

    protected override void OnModelCreating(ModelBuilder builder)
        => builder.ApplySentinelConfiguration();
}
