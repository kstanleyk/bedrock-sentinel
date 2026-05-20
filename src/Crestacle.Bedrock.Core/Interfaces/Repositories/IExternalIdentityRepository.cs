using Crestacle.Bedrock.Core.Entities;

namespace Crestacle.Bedrock.Core.Interfaces.Repositories;

/// <summary>Persistence contract for external OAuth/OIDC identity links.</summary>
public interface IExternalIdentityRepository
{
    /// <summary>Persists a new external identity link.</summary>
    Task AddAsync(ExternalIdentity identity, CancellationToken ct = default);

    /// <summary>Returns the identity linked to the given provider and provider-assigned user ID, or <see langword="null"/> if not found.</summary>
    Task<ExternalIdentity?> GetByProviderAsync(string provider, string providerUserId, CancellationToken ct = default);

    /// <summary>Returns all external identities linked to <paramref name="userId"/>.</summary>
    Task<IReadOnlyList<ExternalIdentity>> GetForUserAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Removes the external identity link from the store.</summary>
    Task DeleteAsync(ExternalIdentity identity, CancellationToken ct = default);
}
