using Crestacle.Bedrock.Core.Entities;

namespace Crestacle.Bedrock.Core.Interfaces.Repositories;

/// <summary>Persistence contract for API key records.</summary>
public interface IApiKeyRepository
{
    /// <summary>Persists a newly created <see cref="ApiKey"/>.</summary>
    Task AddAsync(ApiKey key, CancellationToken ct = default);

    /// <summary>Returns the key whose hash matches <paramref name="keyHash"/>, or <see langword="null"/> if not found.</summary>
    Task<ApiKey?> GetByHashAsync(string keyHash, CancellationToken ct = default);

    /// <summary>Returns all active API keys belonging to <paramref name="userId"/>.</summary>
    Task<IReadOnlyList<ApiKey>> GetForUserAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Stamps the last-used timestamp for the given key.</summary>
    Task UpdateLastUsedAsync(Guid keyId, CancellationToken ct = default);

    /// <summary>Marks the key as revoked, scoped to the owning user to prevent cross-user revocation.</summary>
    Task RevokeAsync(Guid keyId, Guid userId, CancellationToken ct = default);
}
