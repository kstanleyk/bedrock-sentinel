namespace Crestacle.Bedrock.Core.Exceptions;

/// <summary>
/// Base class for all Bedrock domain exceptions. Maps to a specific HTTP status code
/// in <c>BedrockExceptionMiddleware</c>.
/// </summary>
public abstract class BedrockException : Exception
{
    /// <summary>Machine-readable error code included in the response envelope. See <c>BedrockErrorCodes</c>.</summary>
    public string? Code { get; }

    protected BedrockException(string message, string? code = null) : base(message) { Code = code; }

    protected BedrockException(string message, Exception inner, string? code = null) : base(message, inner) { Code = code; }
}
