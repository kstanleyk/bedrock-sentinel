using Crestacle.Sentinel.Core.Authorization;
using Crestacle.Sentinel.Core.Enums;
using Crestacle.Sentinel.EntityFramework;
using Crestacle.Sentinel.EntityFramework.Seeding;
using Microsoft.EntityFrameworkCore;

namespace Crestacle.Sentinel.Tests.EntityFramework;

/// <summary>
/// Tests for SentinelSeeder covering built-in permission seeding,
/// app-specific seeding, idempotency, and graceful handling of unknown permission IDs.
/// </summary>
public sealed class SentinelSeederTests
{
    private static TestAuthDbContext BuildContext()
    {
        var opts = new DbContextOptionsBuilder<TestAuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestAuthDbContext(opts);
    }

    // Minimal concrete seeder for tests — callers provide their app-specific data.
    private sealed class TestSeeder(
        IAuthDbContext        ctx,
        AppPermission[]       perms,
        RoleDefinition[]      roles) : SentinelSeeder(ctx)
    {
        protected override IEnumerable<AppPermission>  GetPermissions() => perms;
        protected override IEnumerable<RoleDefinition> GetRoles()       => roles;
    }

    private static readonly AppPermission InvoiceRead =
        new(AppFeature.Invoice, AppAction.Read, "Finance", "Read invoices", isBasic: true);

    private static readonly AppPermission InvoiceCreate =
        new(AppFeature.Invoice, AppAction.Create, "Finance", "Create invoices");

    private static class AppFeature
    {
        public const string Invoice = nameof(Invoice);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Built-in permissions
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SeedAsync_AlwaysSeedsBuiltInSentinelPermissions()
    {
        await using var ctx = BuildContext();
        var seeder = new TestSeeder(ctx, [], []);

        await seeder.SeedAsync();

        var permIds = await ctx.Permissions.Select(p => p.Id).ToListAsync();
        permIds.Should().Contain(AppPermission.NameFor(SentinelFeature.User,  AppAction.Read));
        permIds.Should().Contain(AppPermission.NameFor(SentinelFeature.User,  AppAction.Update));
        permIds.Should().Contain(AppPermission.NameFor(SentinelFeature.Role,  AppAction.Read));
        permIds.Should().Contain(AppPermission.NameFor(SentinelFeature.Role,  AppAction.Update));
        permIds.Should().Contain(AppPermission.NameFor(SentinelFeature.Audit, AppAction.Read));
    }

    [Fact]
    public async Task SeedAsync_SeedsAppPermissionsAlongsideBuiltIns()
    {
        await using var ctx = BuildContext();
        var seeder = new TestSeeder(ctx, [InvoiceRead, InvoiceCreate], []);

        await seeder.SeedAsync();

        var permIds = await ctx.Permissions.Select(p => p.Id).ToListAsync();
        permIds.Should().Contain(AppPermission.NameFor(AppFeature.Invoice, AppAction.Read));
        permIds.Should().Contain(AppPermission.NameFor(AppFeature.Invoice, AppAction.Create));
        // Built-ins still present.
        permIds.Should().Contain(AppPermission.NameFor(SentinelFeature.User, AppAction.Read));
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Idempotency
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SeedAsync_CalledTwice_DoesNotDuplicatePermissions()
    {
        await using var ctx = BuildContext();
        var seeder = new TestSeeder(ctx, [InvoiceRead], []);

        await seeder.SeedAsync();
        await seeder.SeedAsync(); // second call must be a no-op

        var count = await ctx.Permissions.CountAsync();
        // 5 built-ins + 1 app permission = 6 total, never 12.
        count.Should().Be(6, "seeding twice must not create duplicate rows");
    }

    [Fact]
    public async Task SeedAsync_CalledTwice_DoesNotDuplicateRoles()
    {
        await using var ctx = BuildContext();
        var adminPermId = AppPermission.NameFor(AppFeature.Invoice, AppAction.Read);
        var roles       = new[] { new RoleDefinition("Admin", "Administrator", RoleType.Default, [adminPermId]) };
        var seeder      = new TestSeeder(ctx, [InvoiceRead], roles);

        await seeder.SeedAsync();
        await seeder.SeedAsync();

        var roleCount = await ctx.Roles.CountAsync();
        roleCount.Should().Be(1, "seeding twice must not create duplicate role rows");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Role → Permission wiring
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SeedAsync_CreatesRoleWithCorrectPermissionAssignments()
    {
        await using var ctx = BuildContext();
        var adminPermId = AppPermission.NameFor(AppFeature.Invoice, AppAction.Read);
        var roles       = new[] { new RoleDefinition("Admin", "Administrator", RoleType.Default, [adminPermId]) };
        var seeder      = new TestSeeder(ctx, [InvoiceRead], roles);

        await seeder.SeedAsync();

        var role = await ctx.Roles.FirstAsync(r => r.Name == "Admin");
        var links = await ctx.RolePermissions
            .Where(rp => rp.RoleId == role.Id)
            .Select(rp => rp.PermissionId)
            .ToListAsync();

        links.Should().ContainSingle().Which.Should().Be(adminPermId);
    }

    [Fact]
    public async Task SeedAsync_SkipsUnknownPermissionId_WhenWiringRole()
    {
        await using var ctx = BuildContext();
        // Role definition references a permission that is NOT in GetPermissions() or built-ins.
        var roles  = new[] { new RoleDefinition("Orphan", "Orphan Role", RoleType.Default,
                                ["Permission.DoesNotExist.Read"]) };
        var seeder = new TestSeeder(ctx, [], roles);

        // Must not throw — the unknown permission ID is silently skipped.
        await seeder.SeedAsync();

        var role  = await ctx.Roles.FirstAsync(r => r.Name == "Orphan");
        var links = await ctx.RolePermissions.Where(rp => rp.RoleId == role.Id).CountAsync();
        links.Should().Be(0, "a permission that does not exist in the DB should be skipped");
    }
}
