using Crestacle.Bedrock.Core.DTOs;
using Crestacle.Bedrock.Core.Exceptions;

namespace Crestacle.Bedrock.Core.Interfaces.Services;

/// <summary>
/// Admin-level credential management operations. All mutating methods write an audit entry
/// recording the acting admin's ID alongside the target user.
/// </summary>
public interface IBedrockAdminService
{
    /// <summary>Returns a paged list of all credential summaries ordered by creation date.</summary>
    /// <param name="page">The one-based page index.</param>
    /// <param name="pageSize">The maximum number of records to return per page.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="PagedResult{T}"/> of <see cref="CredentialSummary"/> records.</returns>
    Task<PagedResult<CredentialSummary>> GetUsersAsync(
        int page, int pageSize, CancellationToken ct = default);

    /// <summary>Returns the full credential detail for <paramref name="userId"/>.</summary>
    /// <param name="userId">The user to look up.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The <see cref="CredentialDetail"/> for the specified user.</returns>
    /// <exception cref="BedrockNotFoundException">Thrown when the user does not exist.</exception>
    Task<CredentialDetail> GetUserAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Administratively locks the account, preventing login until unlocked.</summary>
    /// <param name="adminId">The ID of the admin performing the action, recorded in the audit log.</param>
    /// <param name="userId">The user to lock.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="BedrockNotFoundException">Thrown when the user does not exist.</exception>
    Task LockUserAsync(Guid adminId, Guid userId, CancellationToken ct = default);

    /// <summary>Clears an administrative lock so the user can log in again.</summary>
    /// <param name="adminId">The ID of the admin performing the action, recorded in the audit log.</param>
    /// <param name="userId">The user to unlock.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="BedrockNotFoundException">Thrown when the user does not exist.</exception>
    Task UnlockUserAsync(Guid adminId, Guid userId, CancellationToken ct = default);

    /// <summary>Disables MFA and invalidates all recovery codes for the user.</summary>
    /// <param name="adminId">The ID of the admin performing the action, recorded in the audit log.</param>
    /// <param name="userId">The user whose MFA to reset.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="BedrockNotFoundException">Thrown when the user does not exist.</exception>
    Task ResetMfaAsync(Guid adminId, Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Forces an immediate password expiry so the user must set a new password on next login.
    /// Sets <c>PasswordExpiresAt = DateTime.UtcNow</c>.
    /// </summary>
    /// <param name="adminId">The ID of the admin performing the action, recorded in the audit log.</param>
    /// <param name="userId">The user whose password to expire.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="BedrockNotFoundException">Thrown when the user does not exist.</exception>
    Task ExpirePasswordAsync(Guid adminId, Guid userId, CancellationToken ct = default);

    /// <summary>Revokes all active refresh tokens and sessions for the user.</summary>
    /// <param name="adminId">The ID of the admin performing the action, recorded in the audit log.</param>
    /// <param name="userId">The user whose sessions to revoke.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="BedrockNotFoundException">Thrown when the user does not exist.</exception>
    Task RevokeAllSessionsAsync(Guid adminId, Guid userId, CancellationToken ct = default);
}
