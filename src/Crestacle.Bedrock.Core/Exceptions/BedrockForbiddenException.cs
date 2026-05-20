using Crestacle.Bedrock.Core;

namespace Crestacle.Bedrock.Core.Exceptions;

/// <summary>
/// Thrown when an authenticated user lacks sufficient scope or step-up to perform an operation.
/// Maps to HTTP 403 Forbidden.
/// </summary>
public sealed class BedrockForbiddenException : BedrockException
{
    public BedrockForbiddenException(string message = "Access denied.") : base(message, BedrockErrorCodes.Forbidden) { }
}
