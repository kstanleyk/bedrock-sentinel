using Crestacle.Sentinel.Core.Authorization;
using Crestacle.Sentinel.Core.Entities;
using Crestacle.Sentinel.Core.Enums;
using Crestacle.Sentinel.Core.Interfaces;
using Crestacle.Sentinel.EntityFramework.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Crestacle.Sentinel.Tests.EntityFramework;

/// <summary>
/// Tests for UserPermissionRepository covering time-bound assignments,
/// soft-delete filtering, role hierarchy, and multi-tenancy.
/// </summary>
public sealed class UserRepositoryTests
{
    private static TestAuthDbContext BuildContext()
    {
        var opts = new DbContextOptionsBuilder<TestAuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestAuthDbContext(opts);
    }

    private static IPermissionCache NullCache()
    {
        var cache = Substitute.For<IPermissionCache>();
        cache.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
             .Returns((HashSet<string>?)null);
        return cache;
    }

    private static ILogger<UserPermissionRepository> NullLog()
        => Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance
               .CreateLogger<UserPermissionRepository>();

    private static ITenantContext NullTenant()
    {
        var ctx = Substitute.For<ITenantContext>();
        ctx.TenantId.Returns((string?)null);
        return ctx;
    }

    private static ITenantContext WithTenant(string tenantId)
    {
        var ctx = Substitute.For<ITenantContext>();
        ctx.TenantId.Returns(tenantId);
        return ctx;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Time-bound assignments
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetPermissionsForUserAsync_ExcludesExpiredAssignment()
    {
        await using var ctx = BuildContext();

        var user = User.Create("identity-exp", "exp@test.com");
        var role = Role.Create("ExpRole", "Expiry Role", RoleType.Default);
        var perm = Permission.Create(new AppPermission("Invoice", "Read", "Finance", "Read invoices"));

        ctx.Users.Add(user);
        ctx.Roles.Add(role);
        ctx.Permissions.Add(perm);

        // Assignment that expired in the past.
        ctx.UserRoles.Add(new UserRole
        {
            Id        = Guid.NewGuid(),
            UserId    = user.Id,
            RoleId    = role.Id,
            CreatedOn = DateTime.UtcNow.AddDays(-2),
            ExpiresOn = DateTime.UtcNow.AddHours(-1), // expired
        });
        ctx.RolePermissions.Add(new RolePermission
        {
            RoleId       = role.Id,
            PermissionId = perm.Id,
            CreatedOn    = DateTime.UtcNow,
        });
        await ctx.SaveChangesAsync();

        var repo   = new UserPermissionRepository(ctx, NullCache(), NullTenant(), NullLog());
        var result = await repo.GetPermissionsForUserAsync("identity-exp");

        result.Should().BeEmpty("an expired assignment should not grant permissions");
    }

    [Fact]
    public async Task GetPermissionsForUserAsync_IncludesActiveTimeBoundAssignment()
    {
        await using var ctx = BuildContext();

        var user = User.Create("identity-future", "future@test.com");
        var role = Role.Create("FutureRole", "Future Role", RoleType.Default);
        var perm = Permission.Create(new AppPermission("Invoice", "Read", "Finance", "Read invoices"));

        ctx.Users.Add(user);
        ctx.Roles.Add(role);
        ctx.Permissions.Add(perm);

        // Assignment that expires in the future — still active.
        ctx.UserRoles.Add(new UserRole
        {
            Id        = Guid.NewGuid(),
            UserId    = user.Id,
            RoleId    = role.Id,
            CreatedOn = DateTime.UtcNow,
            ExpiresOn = DateTime.UtcNow.AddDays(30), // still active
        });
        ctx.RolePermissions.Add(new RolePermission
        {
            RoleId       = role.Id,
            PermissionId = perm.Id,
            CreatedOn    = DateTime.UtcNow,
        });
        await ctx.SaveChangesAsync();

        var repo   = new UserPermissionRepository(ctx, NullCache(), NullTenant(), NullLog());
        var result = await repo.GetPermissionsForUserAsync("identity-future");

        result.Should().ContainSingle().Which.Should().Be("Permission.Invoice.Read");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Soft-delete filtering
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetPermissionsForUserAsync_ExcludesSoftDeletedUserRole()
    {
        await using var ctx = BuildContext();

        var user = User.Create("identity-del", "del@test.com");
        var role = Role.Create("DelRole", "Deleted Role", RoleType.Default);
        var perm = Permission.Create(new AppPermission("Invoice", "Read", "Finance", "Read invoices"));

        ctx.Users.Add(user);
        ctx.Roles.Add(role);
        ctx.Permissions.Add(perm);

        // Soft-deleted user-role row.
        ctx.UserRoles.Add(new UserRole
        {
            Id        = Guid.NewGuid(),
            UserId    = user.Id,
            RoleId    = role.Id,
            CreatedOn = DateTime.UtcNow.AddDays(-1),
            RemovedOn = DateTime.UtcNow.AddHours(-1), // soft-deleted
            RemovedBy = "admin",
        });
        ctx.RolePermissions.Add(new RolePermission
        {
            RoleId       = role.Id,
            PermissionId = perm.Id,
            CreatedOn    = DateTime.UtcNow,
        });
        await ctx.SaveChangesAsync();

        var repo   = new UserPermissionRepository(ctx, NullCache(), NullTenant(), NullLog());
        var result = await repo.GetPermissionsForUserAsync("identity-del");

        result.Should().BeEmpty("a soft-deleted assignment should not grant permissions");
    }

    [Fact]
    public async Task GetPermissionsForUserAsync_ExcludesSoftDeletedRolePermission()
    {
        await using var ctx = BuildContext();

        var user = User.Create("identity-rp-del", "rpdel@test.com");
        var role = Role.Create("RpDelRole", "Role", RoleType.Default);
        var perm = Permission.Create(new AppPermission("Invoice", "Delete", "Finance", "Delete invoice"));

        ctx.Users.Add(user);
        ctx.Roles.Add(role);
        ctx.Permissions.Add(perm);

        ctx.UserRoles.Add(new UserRole
        {
            Id        = Guid.NewGuid(),
            UserId    = user.Id,
            RoleId    = role.Id,
            CreatedOn = DateTime.UtcNow,
        });

        // Permission soft-deleted from the role.
        ctx.RolePermissions.Add(new RolePermission
        {
            RoleId       = role.Id,
            PermissionId = perm.Id,
            CreatedOn    = DateTime.UtcNow.AddDays(-1),
            RemovedOn    = DateTime.UtcNow.AddHours(-1), // soft-deleted
            RemovedBy    = "admin",
        });
        await ctx.SaveChangesAsync();

        var repo   = new UserPermissionRepository(ctx, NullCache(), NullTenant(), NullLog());
        var result = await repo.GetPermissionsForUserAsync("identity-rp-del");

        result.Should().BeEmpty("a soft-deleted role-permission should not grant permissions");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Role hierarchy
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetPermissionsForUserAsync_InheritsPermissionsFromParentRole()
    {
        await using var ctx = BuildContext();

        var user   = User.Create("identity-hier", "hier@test.com");
        var parent = Role.Create("Parent", "Parent Role", RoleType.Default);
        var child  = Role.Create("Child", "Child Role", RoleType.Default);
        var perm   = Permission.Create(new AppPermission("Invoice", "Read", "Finance", "Read invoices"));

        ctx.Users.Add(user);
        ctx.Roles.Add(parent);
        ctx.Roles.Add(child);
        ctx.Permissions.Add(perm);
        await ctx.SaveChangesAsync(); // save to get IDs

        // Set child's parent.
        child.SetParentRole(parent.Id);
        await ctx.SaveChangesAsync();

        // User is assigned only to the child role.
        ctx.UserRoles.Add(new UserRole
        {
            Id        = Guid.NewGuid(),
            UserId    = user.Id,
            RoleId    = child.Id,
            CreatedOn = DateTime.UtcNow,
        });

        // Permission is on the parent role only.
        ctx.RolePermissions.Add(new RolePermission
        {
            RoleId       = parent.Id,
            PermissionId = perm.Id,
            CreatedOn    = DateTime.UtcNow,
        });
        await ctx.SaveChangesAsync();

        var repo   = new UserPermissionRepository(ctx, NullCache(), NullTenant(), NullLog());
        var result = await repo.GetPermissionsForUserAsync("identity-hier");

        result.Should().ContainSingle().Which.Should().Be("Permission.Invoice.Read",
            "permissions from parent roles should be inherited");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Multi-tenancy
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetPermissionsForUserAsync_ReturnsPermissions_ForMatchingTenant()
    {
        await using var ctx = BuildContext();

        // All data in tenant-1 (user, role, assignments all scoped to the same tenant).
        var user = User.Create("identity-mt1", "mt1@test.com", tenantId: "tenant-1");
        var role = Role.Create("MT1Role", "MT1 Role", RoleType.Default, tenantId: "tenant-1");
        var perm = Permission.Create(new AppPermission("Invoice", "Read", "Finance", "Read invoices"));

        ctx.Users.Add(user);
        ctx.Roles.Add(role);
        ctx.Permissions.Add(perm);
        ctx.UserRoles.Add(new UserRole
        {
            Id        = Guid.NewGuid(),
            UserId    = user.Id,
            RoleId    = role.Id,
            CreatedOn = DateTime.UtcNow,
            TenantId  = "tenant-1",
        });
        ctx.RolePermissions.Add(new RolePermission
        {
            RoleId       = role.Id,
            PermissionId = perm.Id,
            CreatedOn    = DateTime.UtcNow,
            TenantId     = "tenant-1",
        });
        await ctx.SaveChangesAsync();

        var repo   = new UserPermissionRepository(ctx, NullCache(), WithTenant("tenant-1"), NullLog());
        var result = await repo.GetPermissionsForUserAsync("identity-mt1");

        result.Should().ContainSingle().Which.Should().Be("Permission.Invoice.Read");
    }

    [Fact]
    public async Task GetPermissionsForUserAsync_ReturnsEmpty_ForDifferentTenant()
    {
        await using var ctx = BuildContext();

        // All data belongs to tenant-1.
        var user = User.Create("identity-mt2", "mt2@test.com", tenantId: "tenant-1");
        var role = Role.Create("MT2Role", "MT2 Role", RoleType.Default, tenantId: "tenant-1");
        var perm = Permission.Create(new AppPermission("Invoice", "Create", "Finance", "Create invoice"));

        ctx.Users.Add(user);
        ctx.Roles.Add(role);
        ctx.Permissions.Add(perm);
        ctx.UserRoles.Add(new UserRole
        {
            Id        = Guid.NewGuid(),
            UserId    = user.Id,
            RoleId    = role.Id,
            CreatedOn = DateTime.UtcNow,
            TenantId  = "tenant-1",
        });
        ctx.RolePermissions.Add(new RolePermission
        {
            RoleId       = role.Id,
            PermissionId = perm.Id,
            CreatedOn    = DateTime.UtcNow,
            TenantId     = "tenant-1",
        });
        await ctx.SaveChangesAsync();

        // Query as tenant-2 — user is in tenant-1, so the lookup finds nothing.
        var repo   = new UserPermissionRepository(ctx, NullCache(), WithTenant("tenant-2"), NullLog());
        var result = await repo.GetPermissionsForUserAsync("identity-mt2");

        result.Should().BeEmpty("tenant-2 cannot see tenant-1's data");
    }
}
