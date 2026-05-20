using Crestacle.Bedrock.Core.Entities;

namespace Crestacle.Bedrock.Core.Interfaces.Services;

/// <summary>Evaluates the mandatory MFA policy for a given credential.</summary>
public interface IMfaPolicyService
{
    /// <summary>Returns true when mandatory MFA is configured (any MandatoryRoles present).</summary>
    /// <param name="credential">The credential to evaluate.</param>
    /// <returns><see langword="true"/> when at least one mandatory role is configured and the credential holds that role.</returns>
    bool IsMfaRequired(UserCredential credential);

    /// <summary>Returns true when the grace period has been set and has not yet expired.</summary>
    /// <param name="credential">The credential to check.</param>
    /// <returns><see langword="true"/> when <c>MfaGracePeriodEnd</c> is set and in the future.</returns>
    bool IsInGracePeriod(UserCredential credential);

    /// <summary>Returns true when the grace period was set and has since expired.</summary>
    /// <param name="credential">The credential to check.</param>
    /// <returns><see langword="true"/> when <c>MfaGracePeriodEnd</c> is set and in the past.</returns>
    bool GracePeriodExpired(UserCredential credential);

    /// <summary>Computes the grace period end date as UtcNow + GracePeriodDays.</summary>
    /// <returns>The UTC timestamp at which the grace period ends for a user enrolled today.</returns>
    DateTime ComputeGracePeriodEnd();

    /// <summary>
    /// Sets the MFA grace period on <paramref name="credential"/> using the configured
    /// <c>GracePeriodDays</c> and returns the computed end timestamp.
    /// </summary>
    /// <param name="credential">The credential whose grace period to initialise.</param>
    /// <returns>The UTC timestamp at which the grace period ends.</returns>
    DateTime SetGracePeriod(UserCredential credential);
}
