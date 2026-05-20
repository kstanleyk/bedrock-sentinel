using Crestacle.Bedrock.Core.Interfaces;
using Crestacle.Bedrock.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Crestacle.Bedrock.Tests.Integration.Infrastructure;

internal sealed class TestBedrockContext : BedrockContext
{
    public TestBedrockContext(DbContextOptions options, ITenantContext? tenantContext = null)
        : base(options, tenantContext) { }
}
