namespace Crestacle.Sentinel.EntityFramework;

/// <summary>
/// Configuration options for Sentinel's EF repository layer.
/// Pass an <see cref="System.Action{SentinelOptions}"/> to
/// <c>AddSentinelRepositories&lt;TContext&gt;(configure)</c> to customise.
/// </summary>
public sealed class SentinelOptions
{
    /// <summary>
    /// How long resolved permission sets are cached in memory per user.
    /// Defaults to <b>5 minutes</b>.
    /// <para>
    /// Set to <see cref="TimeSpan.Zero"/> to disable caching entirely — every
    /// authorization check will query the database on every request. This guarantees
    /// that a revoked role takes effect on the very next request, at the cost of
    /// additional DB load. Recommended for high-security deployments or any scenario
    /// where immediate revocation is a compliance requirement.
    /// </para>
    /// <para>
    /// For multi-instance deployments the in-memory cache is per-process.
    /// Replace <c>IPermissionCache</c> with a distributed implementation (e.g. Redis)
    /// and set this TTL to whatever your SLA allows for propagation lag.
    /// </para>
    /// </summary>
    public TimeSpan PermissionCacheTtl { get; set; } = TimeSpan.FromMinutes(5);
}
