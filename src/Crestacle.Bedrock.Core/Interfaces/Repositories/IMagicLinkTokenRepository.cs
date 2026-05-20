using Crestacle.Bedrock.Core.Entities;

namespace Crestacle.Bedrock.Core.Interfaces.Repositories;

/// <summary>Persistence contract for <see cref="MagicLinkToken"/>.</summary>
public interface IMagicLinkTokenRepository
{
    /// <summary>Returns the magic-link token whose SHA-256 hash matches <paramref name="tokenHash"/>, or <see langword="null"/> if not found.</summary>
    Task<MagicLinkToken?> GetByHashAsync(string tokenHash, CancellationToken ct = default);

    /// <summary>Persists a newly generated magic-link token.</summary>
    Task AddAsync(MagicLinkToken token, CancellationToken ct = default);

    /// <summary>Persists state changes to an existing magic-link token (e.g., marking it used).</summary>
    Task UpdateAsync(MagicLinkToken token, CancellationToken ct = default);

    /// <summary>Marks all outstanding magic-link tokens for <paramref name="userId"/> as invalid.</summary>
    Task InvalidateAllForUserAsync(Guid userId, CancellationToken ct = default);
}
