namespace Crestacle.Bedrock.Core.Exceptions;

/// <summary>
/// Thrown when a request fails domain validation. Maps to HTTP 400 Bad Request.
/// </summary>
public sealed class BedrockValidationException : BedrockException
{
    /// <summary>Human-readable descriptions of each validation failure.</summary>
    public IReadOnlyList<string> Errors { get; }

    public BedrockValidationException(string message, string? code = null)
        : base(message, code)
    {
        Errors = [message];
    }

    public BedrockValidationException(IReadOnlyList<string> errors)
        : base(string.Join("; ", errors))
    {
        Errors = errors;
    }
}
