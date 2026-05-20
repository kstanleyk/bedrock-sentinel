using Crestacle.Sentinel.Core.Entities;
using Crestacle.Sentinel.Core.Enums;
using Crestacle.Sentinel.Core.Interfaces;
using Crestacle.Sentinel.EntityFramework.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Crestacle.Sentinel.Tests.EntityFramework;

/// <summary>
/// Integration tests for PendingAssignmentRepository covering the dual-approval flow.
/// The key regression suite (RoleExpiresOn section) guards against the v1.8.0 bug
/// where time-bound assignments silently lost their expiry through the 4-Eyes flow.
/// </summary>
public sealed class PendingAssignmentRepositoryTests
{
    private static TestAuthDbContext BuildContext()
    {
        var opts = new DbContextOptionsBuilder<TestAuthDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestAuthDbContext(opts);
    }

    private static ICurrentActor ActorWith(string identityId)
    {
        var actor = Substitute.For<ICurrentActor>();
        actor.IdentityId.Returns(identityId);
        actor.IpAddress.Returns("127.0.0.1");
        actor.UserAgent.Returns("test");
        return actor;
    }

    private static ITenantContext NullTenant()
    {
        var ctx = Substitute.For<ITenantContext>();
        ctx.TenantId.Returns((string?)null);
        return ctx;
    }

    private static IPermissionCache NullCache()
    {
        var cache = Substitute.For<IPermissionCache>();
        cache.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
             .Returns((HashSet<string>?)null);
        return cache;
    }

    // Simple no-op publisher — avoids NSubstitute generic-method quirks.
    private sealed class NullPublisher : ISentinelEventPublisher
    {
        public Task PublishAsync<TEvent>(TEvent evt, CancellationToken ct = default)
            where TEvent : class => Task.CompletedTask;
    }

    private static ILogger<PendingAssignmentRepository> NullLog()
        => Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance
               .CreateLogger<PendingAssignmentRepository>();

    private static PendingAssignmentRepository Repo(TestAuthDbContext ctx, string actorId)
        => new(ctx, ActorWith(actorId), NullCache(), NullTenant(), new NullPublisher(), NullLog());

    // ──────────────────────────────────────────────────────────────────────────
    // RoleExpiresOn — regression guard for the v1.8.0 bug fix
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_WithRoleExpiresOn_PersistsExpiryToPendingAssignment()
    {
        await using var ctx = BuildContext();
        var roleExp = DateTime.UtcNow.AddDays(30);

        var pending = await Repo(ctx, "requestor-1")
            .CreateAsync(Guid.NewGuid(), Guid.NewGuid(), roleExp);

        pending.RoleExpiresOn.Should().BeCloseTo(roleExp, TimeSpan.FromSeconds(1),
            "RoleExpiresOn must be persisted to the PendingAssignment row");
    }

    [Fact]
    public async Task ApproveAsync_WithRoleExpiresOn_AppliesExpiryToCreatedUserRole()
    {
        await using var ctx = BuildContext();

        var user = User.Create("user-identity", "user@test.com");
        var role = Role.Create("TimedRole", "Timed Role", RoleType.Default);
        ctx.Users.Add(user);
        ctx.Roles.Add(role);
        await ctx.SaveChangesAsync();

        var roleExp = DateTime.UtcNow.AddDays(14);
        var pending = await Repo(ctx, "requestor-1").CreateAsync(user.Id, role.Id, roleExp);

        var approved = await Repo(ctx, "approver-1").ApproveAsync(pending.Id);

        approved.Should().BeTrue();
        var userRole = await ctx.UserRoles.SingleAsync(ur => ur.UserId == user.Id);
        userRole.ExpiresOn.Should().BeCloseTo(roleExp, TimeSpan.FromSeconds(1),
            "the time-bound expiry from the original request must survive the 4-Eyes approval flow");
    }

    [Fact]
    public async Task ApproveAsync_WithNoRoleExpiresOn_CreatesPermanentUserRole()
    {
        await using var ctx = BuildContext();

        var user = User.Create("perm-identity", "perm@test.com");
        var role = Role.Create("PermRole", "Permanent Role", RoleType.Default);
        ctx.Users.Add(user);
        ctx.Roles.Add(role);
        await ctx.SaveChangesAsync();

        var pending  = await Repo(ctx, "requestor-1").CreateAsync(user.Id, role.Id, roleExpiresOn: null);
        var approved = await Repo(ctx, "approver-1").ApproveAsync(pending.Id);

        approved.Should().BeTrue();
        var userRole = await ctx.UserRoles.SingleAsync(ur => ur.UserId == user.Id);
        userRole.ExpiresOn.Should().BeNull("omitting expiresOn must result in a permanent assignment");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // 4-Eyes enforcement
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ApproveAsync_BySameActorAsRequestor_ReturnsFalse()
    {
        await using var ctx = BuildContext();

        var user = User.Create("same-actor", "same@test.com");
        var role = Role.Create("AnyRole", "Any Role", RoleType.Default);
        ctx.Users.Add(user);
        ctx.Roles.Add(role);
        await ctx.SaveChangesAsync();

        var pending  = await Repo(ctx, "actor-1").CreateAsync(user.Id, role.Id);
        var approved = await Repo(ctx, "actor-1").ApproveAsync(pending.Id);

        approved.Should().BeFalse("an actor must not approve their own request (4-Eyes principle)");
        ctx.UserRoles.Should().BeEmpty("no UserRole must be created when approval is blocked");
    }
}
