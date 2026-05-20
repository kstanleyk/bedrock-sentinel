namespace Crestacle.Bedrock.Core.Interfaces.Services;

/// <summary>
/// Called by Bedrock immediately before signing an access token.
/// Allows the consuming application to inject additional claims (e.g. employee ID,
/// department) and to supply role names — without modifying the library.
/// </summary>
/// <remarks>
/// <para>
/// Implement <em>both</em> methods when your application uses role-based authorization:
/// <list type="bullet">
///   <item><see cref="EnrichAsync"/> — arbitrary key/value claims embedded verbatim in the token.</item>
///   <item><see cref="GetRolesAsync"/> — role names embedded as <c>role</c> claims.</item>
/// </list>
/// </para>
/// <para>
/// Both methods are called on every token issuance <em>and</em> on every refresh rotation,
/// so the issued token always reflects the user's current roles and claims.
/// If you only implement <see cref="EnrichAsync"/> and leave <see cref="GetRolesAsync"/>
/// at its default (empty), roles passed at <see cref="IRefreshTokenService.IssueAsync"/>
/// call-site are <b>not</b> carried forward to refreshed tokens — the consuming application
/// must return roles from <see cref="GetRolesAsync"/> to preserve them across refreshes.
/// </para>
/// </remarks>
public interface IBedrockClaimsEnricher
{
    /// <summary>
    /// Returns a dictionary of extra claims to embed in the access token for the given user.
    /// Return an empty dictionary to add no additional claims.
    /// </summary>
    /// <param name="userId">The user for whom the access token is being issued.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A dictionary mapping claim type names to string values; may be empty but must not be <c>null</c>.</returns>
    Task<IDictionary<string, string>> EnrichAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Returns the role names to embed as <c>role</c> claims in the access token for the given user.
    /// Called on every token issuance and every refresh rotation.
    /// </summary>
    /// <param name="userId">The user for whom the access token is being issued.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// The role names to embed. Return an empty sequence when the user has no roles.
    /// The default implementation returns an empty sequence.
    /// </returns>
    Task<IEnumerable<string>> GetRolesAsync(Guid userId, CancellationToken ct = default)
        => Task.FromResult(Enumerable.Empty<string>());
}
