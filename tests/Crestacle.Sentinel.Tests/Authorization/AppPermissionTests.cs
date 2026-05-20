using Crestacle.Sentinel.Core.Authorization;
using FluentAssertions;

namespace Crestacle.Sentinel.Tests.Authorization;

public sealed class AppPermissionTests
{
    [Fact]
    public void NameFor_ReturnsCorrectFormat()
    {
        var name = AppPermission.NameFor("Invoice", "Read");
        name.Should().Be("Permission.Invoice.Read");
    }

    [Fact]
    public void Name_Property_MatchesNameFor()
    {
        var perm = new AppPermission("Invoice", "Read", "Finance", "Read invoices");
        perm.Name.Should().Be(AppPermission.NameFor("Invoice", "Read"));
    }

    [Theory]
    [InlineData("Estate",  "Create")]
    [InlineData("User",    "Delete")]
    [InlineData("Payroll", "Update")]
    public void NameFor_AlwaysStartsWithPermissionPrefix(string feature, string action)
    {
        var name = AppPermission.NameFor(feature, action);
        name.Should().StartWith(AppClaim.Permission + ".");
    }
}
