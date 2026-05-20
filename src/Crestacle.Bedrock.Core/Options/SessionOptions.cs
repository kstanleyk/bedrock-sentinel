namespace Crestacle.Bedrock.Core.Options;

/// <summary>Per-user session management settings.</summary>
public sealed class SessionOptions
{
    /// <summary>
    /// Maximum number of concurrent active sessions per user.
    /// When the limit is reached the oldest session is evicted on new login. Default: 5.
    /// </summary>
    public int MaxConcurrentSessions { get; set; } = 5;

    /// <summary>
    /// Hard cap on how long any single session may remain active, regardless of how often
    /// it is refreshed. Once <c>Session.CreatedAt + AbsoluteRefreshExpiry</c> is in the past
    /// the next refresh attempt is rejected with <c>session_expired</c> and the user must
    /// re-authenticate. <c>null</c> (default) disables the absolute cap.
    /// </summary>
    public TimeSpan? AbsoluteRefreshExpiry { get; set; }
}
