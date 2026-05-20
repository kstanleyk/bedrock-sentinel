using Crestacle.Sentinel.AspNetCore.Authorization;
using Crestacle.Sentinel.Core.Authorization;
using FluentAssertions;

namespace Crestacle.Sentinel.Tests.Authorization;

public sealed class MustHavePermissionAttributeTests
{
    [Fact]
    public void Attribute_SetsPolicy_ToCorrectPermissionName()
    {
        var attr = new MustHavePermissionAttribute("Invoice", AppAction.Read);
        attr.Policy.Should().Be("Permission.Invoice.Read");
    }

    [Fact]
    public void Attribute_Policy_MatchesNameFor()
    {
        var attr = new MustHavePermissionAttribute("Estate", AppAction.Create);
        attr.Policy.Should().Be(AppPermission.NameFor("Estate", AppAction.Create));
    }
}
