using System.Security.Claims;
using Crestacle.Sentinel.AspNetCore.Authorization;
using Crestacle.Sentinel.Core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using NSubstitute;

namespace Crestacle.Sentinel.Tests.Authorization;

public sealed class PermissionAuthorizationHandlerTests
{
    private readonly IUserPermissionRepository _repo = Substitute.For<IUserPermissionRepository>();
    private readonly PermissionAuthorizationHandler _handler;

    public PermissionAuthorizationHandlerTests()
        => _handler = new PermissionAuthorizationHandler(_repo);

    [Fact]
    public async Task HandleRequirement_Succeeds_WhenUserHasPermission()
    {
        const string identityId = "user-123";
        const string permission = "Permission.Invoice.Read";

        _repo.GetPermissionsForUserAsync(identityId, Arg.Any<CancellationToken>())
             .Returns([permission, "Permission.Invoice.Create"]);

        var user    = BuildUser(identityId);
        var context = BuildAuthContext(user, permission);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirement_Fails_WhenUserLacksPermission()
    {
        const string identityId = "user-123";
        const string permission = "Permission.Invoice.Delete";

        _repo.GetPermissionsForUserAsync(identityId, Arg.Any<CancellationToken>())
             .Returns(["Permission.Invoice.Read"]);

        var user    = BuildUser(identityId);
        var context = BuildAuthContext(user, permission);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirement_Fails_WhenSubjectClaimIsMissing()
    {
        var user    = new ClaimsPrincipal(new ClaimsIdentity());
        var context = BuildAuthContext(user, "Permission.Invoice.Read");

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
        await _repo.DidNotReceive().GetPermissionsForUserAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    private static ClaimsPrincipal BuildUser(string identityId)
        => new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, identityId)], "test"));

    private static AuthorizationHandlerContext BuildAuthContext(ClaimsPrincipal user, string permission)
        => new([new PermissionRequirement(permission)], user, null);
}
