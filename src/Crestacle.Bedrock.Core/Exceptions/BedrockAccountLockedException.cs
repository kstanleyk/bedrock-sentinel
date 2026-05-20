using Crestacle.Bedrock.Core;

namespace Crestacle.Bedrock.Core.Exceptions;

/// <summary>
/// Thrown when authentication is attempted against a locked account.
/// Maps to HTTP 423 Locked. The <see cref="LockoutEnd"/> value is used to
/// populate the <c>Retry-After</c> response header.
/// </summary>
public sealed class BedrockAccountLockedException : BedrockException
{
    /// <summary>UTC timestamp when the lockout expires.</summary>
    public DateTime LockoutEnd { get; }

    public BedrockAccountLockedException(DateTime lockoutEnd)
        : base($"Account is locked until {lockoutEnd:O}.", BedrockErrorCodes.AccountLocked)
    {
        LockoutEnd = lockoutEnd;
    }
}
