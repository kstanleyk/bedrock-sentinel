namespace Crestacle.Sentinel.Core.Exceptions;

/// <summary>
/// Thrown when a concurrent write conflict is detected on a Sentinel entity.
/// Surface this as HTTP 409 Conflict in your exception-handling middleware.
/// </summary>
public sealed class SentinelConcurrencyException : Exception
{
    public SentinelConcurrencyException(string message, Exception inner)
        : base(message, inner) { }
}
