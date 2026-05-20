using Crestacle.Bedrock.Core.DTOs;
using Crestacle.Bedrock.Core.Entities;

namespace Crestacle.Bedrock.Core.Interfaces.Services;

/// <summary>
/// WebAuthn/FIDO2 passkey registration, authentication, and credential management.
/// </summary>
public interface IPasskeyService
{
    /// <summary>Begins a passkey registration ceremony. Returns <c>CredentialCreateOptions</c> JSON.</summary>
    /// <param name="userId">The user registering the passkey.</param>
    /// <param name="username">The display name included in the options (typically the user's email or handle).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A JSON string containing <c>PublicKeyCredentialCreationOptions</c> to pass to the WebAuthn API.</returns>
    Task<string> BeginRegistrationAsync(Guid userId, string username, CancellationToken ct = default);

    /// <summary>Completes the registration ceremony and persists the new credential.</summary>
    /// <param name="userId">The user completing the registration.</param>
    /// <param name="attestationResponseJson">The JSON-serialised <c>AuthenticatorAttestationResponse</c> from the client.</param>
    /// <param name="friendlyName">Optional human-readable label for the passkey (e.g. "My YubiKey").</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The persisted <see cref="PasskeyCredential"/> record.</returns>
    Task<PasskeyCredential> CompleteRegistrationAsync(
        Guid userId,
        string attestationResponseJson,
        string? friendlyName,
        CancellationToken ct = default);

    /// <summary>Begins a passkey authentication ceremony. Returns <c>AssertionOptions</c> JSON.</summary>
    /// <param name="email">Optional email to limit the allowCredentials list. Pass <c>null</c> for discoverable-credential (resident key) flow.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A JSON string containing <c>PublicKeyCredentialRequestOptions</c> to pass to the WebAuthn API.</returns>
    Task<string> BeginAuthenticationAsync(string? email, CancellationToken ct = default);

    /// <summary>
    /// Completes the authentication ceremony. Verifies the assertion, updates the sign counter,
    /// creates a session, and issues a token pair. Passkey login satisfies both factors.
    /// Throws <see cref="Core.Exceptions.BedrockValidationException"/> on any failure.
    /// </summary>
    /// <param name="assertionResponseJson">The JSON-serialised <c>AuthenticatorAssertionResponse</c> from the client.</param>
    /// <param name="ipAddress">The requesting IP address, stored in the new session.</param>
    /// <param name="userAgent">The User-Agent header of the request, stored in the new session.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="LoginResult"/> containing the issued token pair and MFA status.</returns>
    Task<LoginResult> CompleteAuthenticationAsync(
        string assertionResponseJson,
        string ipAddress,
        string userAgent,
        CancellationToken ct = default);

    /// <summary>Returns all passkeys registered for a user.</summary>
    /// <param name="userId">The user whose passkeys to retrieve.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A read-only list of <see cref="PasskeyCredential"/> records. Never null; empty when the user has no passkeys.</returns>
    Task<IReadOnlyList<PasskeyCredential>> GetPasskeysAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Deletes a passkey by its database ID, verifying it belongs to the requesting user.
    /// </summary>
    /// <param name="passkeyId">The database identifier of the passkey to delete.</param>
    /// <param name="requestingUserId">The authenticated user; the delete is rejected if the passkey belongs to a different user.</param>
    /// <param name="ct">Cancellation token.</param>
    Task DeletePasskeyAsync(Guid passkeyId, Guid requestingUserId, CancellationToken ct = default);
}
