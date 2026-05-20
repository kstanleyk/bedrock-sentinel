using Crestacle.Bedrock.Core;

namespace Crestacle.Bedrock.Core.Exceptions;

/// <summary>
/// Thrown when a required entity does not exist. Maps to HTTP 404 Not Found.
/// </summary>
public sealed class BedrockNotFoundException : BedrockException
{
    public BedrockNotFoundException(string message = "The requested resource was not found.")
        : base(message, BedrockErrorCodes.NotFound) { }

    public BedrockNotFoundException(string entityName, object id)
        : base($"{entityName} with identifier '{id}' was not found.", BedrockErrorCodes.NotFound) { }
}
