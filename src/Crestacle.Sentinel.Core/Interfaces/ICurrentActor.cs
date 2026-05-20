namespace Crestacle.Sentinel.Core.Interfaces;

/// <summary>
/// Provides identity and request context of the user executing the current operation.
/// Used by repositories to stamp audit log entries without coupling to HttpContext directly.
/// </summary>
public interface ICurrentActor
{
    /// <summary>External identity ID from the JWT NameIdentifier claim. Null in background contexts.</summary>
    string? IdentityId { get; }

    /// <summary>Remote IP address, or null when unavailable.</summary>
    string? IpAddress { get; }

    /// <summary>User-Agent header, or null when unavailable.</summary>
    string? UserAgent { get; }
}
