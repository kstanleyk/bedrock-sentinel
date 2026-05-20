namespace Crestacle.Sentinel.Core.Exceptions;

/// <summary>
/// Thrown when a referenced entity (e.g. a permission ID passed to AddPermissionAsync)
/// does not exist in the database.
/// Surface this as HTTP 404 Not Found in your exception-handling middleware.
/// </summary>
public sealed class SentinelNotFoundException : Exception
{
    public SentinelNotFoundException(string message) : base(message) { }
}
