using Crestacle.Bedrock.Core.Enumerations;

namespace Crestacle.Bedrock.Core.DTOs;

/// <summary>
/// Returned when first-factor login succeeds but MFA verification is required.
/// The client must exchange the <see cref="ChallengeToken"/> plus a valid code
/// for a full <see cref="TokenPair"/>.
/// </summary>
public sealed record MfaChallengeResult(
    string ChallengeToken,
    MfaMethod Method,
    DateTime ExpiresAt);
