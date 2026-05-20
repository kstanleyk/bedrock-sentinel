using Crestacle.Bedrock.Core.Entities;

namespace Crestacle.Bedrock.Core.Interfaces.Repositories;

/// <summary>Persistence contract for <see cref="EmailVerificationToken"/>.</summary>
public interface IEmailVerificationTokenRepository
{
    /// <summary>Returns the token whose SHA-256 hash matches <paramref name="tokenHash"/>, or <see langword="null"/> if not found.</summary>
    Task<EmailVerificationToken?> GetByHashAsync(string tokenHash, CancellationToken ct = default);

    /// <summary>Persists a newly created email verification token.</summary>
    Task AddAsync(EmailVerificationToken token, CancellationToken ct = default);

    /// <summary>Persists state changes to an existing email verification token (e.g., marking it used).</summary>
    Task UpdateAsync(EmailVerificationToken token, CancellationToken ct = default);

    /// <summary>Marks all outstanding email verification tokens for <paramref name="userId"/> as invalid.</summary>
    Task InvalidateAllForUserAsync(Guid userId, CancellationToken ct = default);
}
