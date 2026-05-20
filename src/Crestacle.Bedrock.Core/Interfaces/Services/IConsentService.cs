using Crestacle.Bedrock.Core.Entities;

namespace Crestacle.Bedrock.Core.Interfaces.Services;

/// <summary>Records and retrieves user consent for versioned policy documents.</summary>
public interface IConsentService
{
    /// <summary>Creates an immutable consent record for the given user and policy version.</summary>
    /// <param name="userId">The user giving consent.</param>
    /// <param name="policyType">Logical category of the policy (e.g. "PrivacyPolicy", "TermsOfService").</param>
    /// <param name="policyVersion">The version string of the policy being accepted.</param>
    /// <param name="ipAddress">The IP address of the request, stored for audit purposes.</param>
    /// <param name="userAgent">The User-Agent header of the request, stored for audit purposes.</param>
    /// <param name="tenantId">Optional tenant scope for multi-tenant deployments.</param>
    /// <param name="ct">Cancellation token.</param>
    Task RecordConsentAsync(
        Guid userId,
        string policyType,
        string policyVersion,
        string ipAddress,
        string userAgent,
        string? tenantId = null,
        CancellationToken ct = default);

    /// <summary>Returns all consent records for the given user, newest first.</summary>
    /// <param name="userId">The user whose consent history to retrieve.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An ordered list of consent records, most recent first.</returns>
    Task<IReadOnlyList<ConsentRecord>> GetConsentHistoryAsync(Guid userId, CancellationToken ct = default);
}
