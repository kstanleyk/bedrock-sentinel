using Crestacle.Bedrock.Core.Enumerations;

namespace Crestacle.Bedrock.Core.DomainEvents;

public sealed record EmailVerificationRequestedEvent(Guid UserId, string Email, DateTime OccurredAt);
public sealed record EmailVerifiedEvent(Guid UserId, DateTime OccurredAt);
public sealed record PasswordChangedEvent(Guid UserId, DateTime OccurredAt);
public sealed record PasswordResetRequestedEvent(Guid UserId, DateTime OccurredAt);
public sealed record PasswordResetCompletedEvent(Guid UserId, DateTime OccurredAt);
public sealed record LoginSucceededEvent(Guid UserId, string IpAddress, bool MfaEnabled, DateTime OccurredAt);
public sealed record MfaEnabledEvent(Guid UserId, MfaMethod Method, DateTime OccurredAt);
public sealed record MfaDisabledEvent(Guid UserId, DateTime OccurredAt);
public sealed record MfaChallengeSucceededEvent(Guid UserId, string IpAddress, DateTime OccurredAt);
public sealed record AnomalyDetectedEvent(Guid UserId, string IpAddress, string FingerprintHash, DateTime OccurredAt);
public sealed record StepUpCompletedEvent(Guid UserId, string IpAddress, DateTime OccurredAt);
public sealed record StepUpFailedEvent(Guid UserId, string IpAddress, DateTime OccurredAt);
