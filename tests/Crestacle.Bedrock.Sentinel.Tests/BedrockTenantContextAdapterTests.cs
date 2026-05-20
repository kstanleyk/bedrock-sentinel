using Crestacle.Bedrock.Sentinel;
using FluentAssertions;
using NSubstitute;
using Xunit;

using BedrockTenant = Crestacle.Bedrock.Core.Interfaces.ITenantContext;

namespace Crestacle.Bedrock.Sentinel.Tests;

public sealed class BedrockTenantContextAdapterTests
{
    [Fact]
    public void TenantId_ForwardsBedrocksGetTenantId()
    {
        var bedrock = Substitute.For<BedrockTenant>();
        bedrock.GetTenantId().Returns("acme");

        var adapter = new BedrockTenantContextAdapter(bedrock);

        adapter.TenantId.Should().Be("acme");
    }

    [Fact]
    public void TenantId_ReturnsNull_WhenBedrockReturnsNull()
    {
        var bedrock = Substitute.For<BedrockTenant>();
        bedrock.GetTenantId().Returns((string?)null);

        var adapter = new BedrockTenantContextAdapter(bedrock);

        adapter.TenantId.Should().BeNull();
    }

    [Fact]
    public void TenantId_CallsGetTenantIdOnEveryAccess()
    {
        var bedrock = Substitute.For<BedrockTenant>();
        bedrock.GetTenantId().Returns("t1", "t2");

        var adapter = new BedrockTenantContextAdapter(bedrock);

        _ = adapter.TenantId;
        _ = adapter.TenantId;

        bedrock.Received(2).GetTenantId();
    }
}
