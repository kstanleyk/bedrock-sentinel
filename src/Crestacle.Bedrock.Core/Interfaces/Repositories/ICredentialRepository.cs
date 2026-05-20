using Crestacle.Bedrock.Core.DTOs;
using Crestacle.Bedrock.Core.Entities;

namespace Crestacle.Bedrock.Core.Interfaces.Repositories;

/// <summary>Persistence contract for <see cref="UserCredential"/>.</summary>
public interface ICredentialRepository
{
    /// <summary>Returns the credential for <paramref name="userId"/>, or <see langword="null"/> if not found.</summary>
    Task<UserCredential?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Returns the credential whose email address matches <paramref name="email"/> (case-insensitive), or <see langword="null"/> if not found.</summary>
    Task<UserCredential?> GetByEmailAsync(string email, CancellationToken ct = default);

    /// <summary>Persists a newly created credential.</summary>
    Task AddAsync(UserCredential credential, CancellationToken ct = default);

    /// <summary>Persists state changes to an existing credential.</summary>
    Task UpdateAsync(UserCredential credential, CancellationToken ct = default);

    /// <summary>Returns <see langword="true"/> if any credential with the given <paramref name="email"/> already exists.</summary>
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);

    /// <summary>Returns a paged list of credential summaries for administrative listing, ordered by creation date descending.</summary>
    Task<PagedResult<CredentialSummary>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
}
