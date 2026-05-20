using Crestacle.Bedrock.Core.Entities;

namespace Crestacle.Bedrock.Core.Interfaces.Repositories;

/// <summary>Persistence contract for <see cref="EmailChangeToken"/>.</summary>
public interface IEmailChangeTokenRepository
{
    /// <summary>Persists a newly created email-change token.</summary>
    Task AddAsync(EmailChangeToken token, CancellationToken ct = default);

    /// <summary>Returns the token whose SHA-256 hash matches <paramref name="tokenHash"/>, or <see langword="null"/> if not found.</summary>
    Task<EmailChangeToken?> GetByHashAsync(string tokenHash, CancellationToken ct = default);

    /// <summary>Persists state changes to an existing email-change token (e.g., marking it used).</summary>
    Task UpdateAsync(EmailChangeToken token, CancellationToken ct = default);

    /// <summary>Marks all outstanding email-change tokens for <paramref name="userId"/> as invalid.</summary>
    Task InvalidateAllForUserAsync(Guid userId, CancellationToken ct = default);
}
