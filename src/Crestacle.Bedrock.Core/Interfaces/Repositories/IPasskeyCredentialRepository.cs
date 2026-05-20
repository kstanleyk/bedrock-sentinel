using Crestacle.Bedrock.Core.Entities;

namespace Crestacle.Bedrock.Core.Interfaces.Repositories;

/// <summary>Persistence contract for WebAuthn/FIDO2 passkey credentials.</summary>
public interface IPasskeyCredentialRepository
{
    /// <summary>Persists a newly registered passkey credential.</summary>
    Task AddAsync(PasskeyCredential credential, CancellationToken ct = default);

    /// <summary>Returns the credential with the given surrogate ID, or <see langword="null"/> if not found.</summary>
    Task<PasskeyCredential?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Returns the credential whose FIDO2 credential ID matches <paramref name="credentialId"/>, or <see langword="null"/> if not found.</summary>
    Task<PasskeyCredential?> GetByCredentialIdAsync(byte[] credentialId, CancellationToken ct = default);

    /// <summary>Returns all passkey credentials registered for <paramref name="userId"/>.</summary>
    Task<IReadOnlyList<PasskeyCredential>> GetForUserAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Updates an existing credential (e.g., to increment the sign-count after successful authentication).</summary>
    Task UpdateAsync(PasskeyCredential credential, CancellationToken ct = default);

    /// <summary>Removes the credential from the store.</summary>
    Task DeleteAsync(PasskeyCredential credential, CancellationToken ct = default);
}
