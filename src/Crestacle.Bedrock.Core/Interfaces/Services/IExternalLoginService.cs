using Crestacle.Bedrock.Core.DTOs;
using Crestacle.Bedrock.Core.Entities;

namespace Crestacle.Bedrock.Core.Interfaces.Services;

/// <summary>Social / external identity federation service.</summary>
public interface IExternalLoginService
{
    /// <summary>
    /// Validates the provider token, resolves the linked user, and issues tokens.
    /// If no <see cref="ExternalIdentity"/> exists for the provider user ID but the
    /// provider claims an email that matches an existing account, it auto-links and proceeds.
    /// Throws <see cref="Core.Exceptions.BedrockNotFoundException"/> when no account can be resolved.
    /// </summary>
    /// <param name="provider">The provider name (e.g. "google", "github") used to select the registered <see cref="IExternalIdentityValidator"/>.</param>
    /// <param name="providerToken">The access or ID token issued by the external provider.</param>
    /// <param name="ipAddress">The requesting IP address, stored in the new session.</param>
    /// <param name="userAgent">The User-Agent header of the request, stored in the new session.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="LoginResult"/> containing the issued token pair and MFA status.</returns>
    Task<LoginResult> ExternalLoginAsync(
        string provider,
        string providerToken,
        string ipAddress,
        string userAgent,
        CancellationToken ct = default);

    /// <summary>
    /// Resolves the linked user from pre-validated claims and issues tokens, skipping provider
    /// token validation. Use this when an external IDP (e.g. OpenIddict) has already validated
    /// the provider token and extracted claims before calling into Bedrock.
    /// If no <see cref="ExternalIdentity"/> exists for the provider user ID but the claims
    /// contain an email that matches an existing account, it auto-links and proceeds.
    /// Throws <see cref="Core.Exceptions.BedrockNotFoundException"/> when no account can be resolved.
    /// </summary>
    /// <param name="provider">The provider name (e.g. "google", "github").</param>
    /// <param name="claims">Pre-validated identity claims supplied by the external IDP.</param>
    /// <param name="ipAddress">The requesting IP address, stored in the new session.</param>
    /// <param name="userAgent">The User-Agent header of the request, stored in the new session.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="LoginResult"/> containing the issued token pair and MFA status.</returns>
    Task<LoginResult> LoginWithClaimsAsync(
        string provider,
        ExternalIdentityClaims claims,
        string ipAddress,
        string userAgent,
        CancellationToken ct = default);

    /// <summary>Links an external provider identity to an existing authenticated user.</summary>
    /// <param name="userId">The authenticated user to link the external identity to.</param>
    /// <param name="provider">The provider name (e.g. "google", "github").</param>
    /// <param name="providerToken">The provider token to validate and extract the external user ID from.</param>
    /// <param name="ct">Cancellation token.</param>
    Task LinkExternalIdentityAsync(
        Guid userId,
        string provider,
        string providerToken,
        CancellationToken ct = default);

    /// <summary>
    /// Links pre-validated external identity claims to an existing authenticated user, skipping
    /// provider token validation. Use this when an external IDP (e.g. OpenIddict) has already
    /// validated the provider token and extracted claims before calling into Bedrock.
    /// Throws <see cref="Core.Exceptions.BedrockValidationException"/> if the external identity
    /// is already linked to any account.
    /// </summary>
    /// <param name="userId">The authenticated user to link the external identity to.</param>
    /// <param name="provider">The provider name (e.g. "google", "github").</param>
    /// <param name="claims">Pre-validated identity claims supplied by the external IDP.</param>
    /// <param name="ct">Cancellation token.</param>
    Task LinkWithClaimsAsync(
        Guid userId,
        string provider,
        ExternalIdentityClaims claims,
        CancellationToken ct = default);

    /// <summary>
    /// Removes a linked external identity. Throws <see cref="Core.Exceptions.BedrockValidationException"/>
    /// if unlinking would leave the account with no credentials.
    /// </summary>
    /// <param name="userId">The authenticated user to unlink from.</param>
    /// <param name="provider">The provider name to unlink.</param>
    /// <param name="ct">Cancellation token.</param>
    Task UnlinkExternalIdentityAsync(
        Guid userId,
        string provider,
        CancellationToken ct = default);

    /// <summary>Returns all external identities linked to the given user.</summary>
    /// <param name="userId">The user whose linked identities to retrieve.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A read-only list of <see cref="ExternalIdentity"/> records. Never null; empty when no external identities are linked.</returns>
    Task<IReadOnlyList<ExternalIdentity>> GetLinkedIdentitiesAsync(
        Guid userId,
        CancellationToken ct = default);
}
