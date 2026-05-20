using Crestacle.Bedrock.Core.Enumerations;

namespace Crestacle.Bedrock.Core.DTOs;

/// <summary>
/// Returned when a step-up challenge is initiated. The client must verify with the
/// appropriate code and the <see cref="ChallengeId"/> to obtain a step-up JWT.
/// </summary>
public sealed record StepUpChallengeResult(
    Guid ChallengeId,
    MfaMethod Method);
