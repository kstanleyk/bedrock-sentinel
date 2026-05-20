namespace Crestacle.Bedrock.Core.Interfaces;

/// <summary>
/// Resolves the current tenant identifier from ambient context (HTTP header, JWT claim,
/// subdomain, etc.). Applied as an EF Core global query filter on every Bedrock entity.
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// Returns the current tenant identifier, or <c>null</c> for single-tenant deployments.
    /// </summary>
    /// <returns>The tenant ID string, or <c>null</c> when not in a multi-tenant context.</returns>
    string? GetTenantId();
}
