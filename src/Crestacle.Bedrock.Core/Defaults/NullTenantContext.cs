using Crestacle.Bedrock.Core.Interfaces;

namespace Crestacle.Bedrock.Core.Defaults;

/// <summary>
/// Single-tenant default: always returns null, meaning no tenant isolation is applied.
/// Replace with a custom implementation for multi-tenant deployments.
/// </summary>
public sealed class NullTenantContext : ITenantContext
{
    public string? GetTenantId() => null;
}
