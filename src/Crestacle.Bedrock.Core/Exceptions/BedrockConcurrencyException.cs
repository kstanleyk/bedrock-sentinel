using Crestacle.Bedrock.Core;

namespace Crestacle.Bedrock.Core.Exceptions;

/// <summary>
/// Thrown when an optimistic concurrency conflict is detected. Maps to HTTP 409 Conflict.
/// </summary>
public sealed class BedrockConcurrencyException : BedrockException
{
    public BedrockConcurrencyException()
        : base("A concurrency conflict occurred. Please retry the operation.", BedrockErrorCodes.ConcurrencyConflict) { }

    public BedrockConcurrencyException(string message)
        : base(message, BedrockErrorCodes.ConcurrencyConflict) { }

    public BedrockConcurrencyException(string message, Exception inner)
        : base(message, inner, BedrockErrorCodes.ConcurrencyConflict) { }
}
