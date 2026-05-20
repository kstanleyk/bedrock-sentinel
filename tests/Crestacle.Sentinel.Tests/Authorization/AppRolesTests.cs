using Crestacle.Sentinel.Core.Authorization;
using FluentAssertions;

namespace Crestacle.Sentinel.Tests.Authorization;

public sealed class AppRolesTests
{
    [Theory]
    [InlineData(AppRoles.Admin)]
    [InlineData(AppRoles.Basic)]
    public void IsDefault_ReturnsTrue_ForBuiltInRoles(string role)
        => AppRoles.IsDefault(role).Should().BeTrue();

    [Theory]
    [InlineData("GeneralManager")]
    [InlineData("FieldSupervisor")]
    [InlineData("CustomRole")]
    public void IsDefault_ReturnsFalse_ForAppSpecificRoles(string role)
        => AppRoles.IsDefault(role).Should().BeFalse();
}
