namespace Crestacle.Sentinel.Core.Interfaces;

/// <summary>
/// Provides the current tenant scope.
/// Return null (default) for single-tenant applications — all queries run without tenant filtering.
/// Implement and register this interface to enable multi-tenancy.
/// </summary>
public interface ITenantContext
{
    string? TenantId { get; }
}
