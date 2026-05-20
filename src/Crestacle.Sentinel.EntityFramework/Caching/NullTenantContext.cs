using Crestacle.Sentinel.Core.Interfaces;

namespace Crestacle.Sentinel.EntityFramework.Caching;

/// <summary>
/// Default tenant context for single-tenant applications.
/// Always returns null — no tenant filtering is applied.
/// </summary>
internal sealed class NullTenantContext : ITenantContext
{
    public string? TenantId => null;
}
