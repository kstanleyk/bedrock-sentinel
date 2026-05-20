using Crestacle.Bedrock.Core.Entities;

namespace Crestacle.Bedrock.Core.Interfaces.Repositories;

/// <summary>Persistence contract for <see cref="Invitation"/>.</summary>
public interface IInvitationRepository
{
    /// <summary>Persists a newly created invitation.</summary>
    Task AddAsync(Invitation invitation, CancellationToken ct = default);

    /// <summary>Returns the invitation whose token hash matches <paramref name="tokenHash"/>, or <see langword="null"/> if not found.</summary>
    Task<Invitation?> GetByHashAsync(string tokenHash, CancellationToken ct = default);

    /// <summary>Persists state changes to an existing invitation (e.g., marking it accepted or revoked).</summary>
    Task UpdateAsync(Invitation invitation, CancellationToken ct = default);
}
