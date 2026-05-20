namespace Crestacle.Sentinel.Core.Authorization;

/// <summary>
/// Built-in feature names used by the Sentinel management endpoints.
/// Apps must include these in their permission registry to enable the management UI.
/// </summary>
public static class SentinelFeature
{
    public const string User  = nameof(User);
    public const string Role  = nameof(Role);
    public const string Audit = nameof(Audit);
}
