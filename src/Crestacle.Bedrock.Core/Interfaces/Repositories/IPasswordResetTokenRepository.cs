using Crestacle.Bedrock.Core.Entities;

namespace Crestacle.Bedrock.Core.Interfaces.Repositories;

/// <summary>Persistence contract for <see cref="PasswordResetToken"/>.</summary>
public interface IPasswordResetTokenRepository
{
    /// <summary>Returns the token whose SHA-256 hash matches <paramref name="tokenHash"/>, or <see langword="null"/> if not found.</summary>
    Task<PasswordResetToken?> GetByHashAsync(string tokenHash, CancellationToken ct = default);

    /// <summary>Persists a newly created password reset token.</summary>
    Task AddAsync(PasswordResetToken token, CancellationToken ct = default);

    /// <summary>Persists state changes to an existing password reset token (e.g., marking it used).</summary>
    Task UpdateAsync(PasswordResetToken token, CancellationToken ct = default);

    /// <summary>Marks all outstanding password reset tokens for <paramref name="userId"/> as invalid.</summary>
    Task InvalidateAllForUserAsync(Guid userId, CancellationToken ct = default);
}
