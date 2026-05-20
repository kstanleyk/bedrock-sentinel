using Crestacle.Bedrock.Core.Entities;
using Crestacle.Bedrock.Core.Exceptions;

namespace Crestacle.Bedrock.Core.Interfaces.Services;

/// <summary>Session management: list active sessions, revoke by ID, revoke all.</summary>
public interface ISessionService
{
    /// <summary>Returns all active (non-revoked) sessions for the specified user.</summary>
    /// <param name="userId">The user whose sessions to retrieve.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A read-only list of active sessions, ordered by creation time.</returns>
    Task<IReadOnlyList<Session>> GetActiveSessionsAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Revokes the session and its associated refresh token.
    /// </summary>
    /// <param name="sessionId">The ID of the session to revoke.</param>
    /// <param name="requestingUserId">The user making the request; must own the session.</param>
    /// <param name="ip">The IP address of the request, recorded in the audit log.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="BedrockNotFoundException">Thrown when no session with <paramref name="sessionId"/> exists.</exception>
    /// <exception cref="BedrockForbiddenException">Thrown when the session belongs to a different user.</exception>
    Task RevokeSessionAsync(Guid sessionId, Guid requestingUserId, string ip, CancellationToken ct = default);

    /// <summary>Revokes all active sessions and their associated refresh tokens for the specified user.</summary>
    /// <param name="userId">The user whose sessions to revoke.</param>
    /// <param name="ip">The IP address of the request, recorded in the audit log.</param>
    /// <param name="ct">Cancellation token.</param>
    Task RevokeAllSessionsAsync(Guid userId, string ip, CancellationToken ct = default);
}
