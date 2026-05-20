using Crestacle.Bedrock.Core.DTOs;

namespace Crestacle.Bedrock.Core.Interfaces.Services;

/// <summary>Application-layer contract for creating, listing, and revoking API keys.</summary>
public interface IApiKeyService
{
    /// <summary>Generates a new API key for <paramref name="userId"/> and returns the raw key (shown once) together with its metadata.</summary>
    /// <param name="userId">The owner of the new key.</param>
    /// <param name="name">Optional human-readable label for the key.</param>
    /// <param name="ipAddress">The requesting IP address, recorded in the audit log.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="CreateApiKeyResult"/> containing the plaintext key and its metadata. The raw key is returned only once and cannot be retrieved again.</returns>
    Task<CreateApiKeyResult> CreateAsync(Guid userId, string? name, string ipAddress, CancellationToken ct = default);

    /// <summary>Returns summary metadata for all API keys owned by <paramref name="userId"/>.</summary>
    /// <param name="userId">The owner whose keys to list.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A read-only list of <see cref="ApiKeySummary"/> records. Never null; empty when the user has no keys.</returns>
    Task<IReadOnlyList<ApiKeySummary>> ListAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Revokes the specified key, ensuring it belongs to <paramref name="userId"/>.</summary>
    /// <param name="keyId">The database identifier of the key to revoke.</param>
    /// <param name="userId">The owner; the operation is a no-op if the key does not belong to this user.</param>
    /// <param name="ipAddress">The requesting IP address, recorded in the audit log.</param>
    /// <param name="ct">Cancellation token.</param>
    Task RevokeAsync(Guid keyId, Guid userId, string ipAddress, CancellationToken ct = default);
}
