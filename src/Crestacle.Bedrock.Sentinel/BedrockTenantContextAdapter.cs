using BedrockTenant  = Crestacle.Bedrock.Core.Interfaces.ITenantContext;
using SentinelTenant = Crestacle.Sentinel.Core.Interfaces.ITenantContext;

namespace Crestacle.Bedrock.Sentinel;

/// <summary>
/// Adapts the Bedrock <see cref="BedrockTenant"/> (method-based) to the Sentinel
/// <see cref="SentinelTenant"/> (property-based) so a single host registration feeds both libraries.
/// </summary>
/// <remarks>
/// Register your concrete <see cref="BedrockTenant"/> implementation first, then call
/// <c>AddSentinel()</c>. The bridge will resolve it here and forward the tenant ID to Sentinel.
/// </remarks>
internal sealed class BedrockTenantContextAdapter(BedrockTenant bedrock) : SentinelTenant
{
    public string? TenantId => bedrock.GetTenantId();
}
