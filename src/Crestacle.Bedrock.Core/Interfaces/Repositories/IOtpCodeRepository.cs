using Crestacle.Bedrock.Core.Entities;
using Crestacle.Bedrock.Core.Enumerations;

namespace Crestacle.Bedrock.Core.Interfaces.Repositories;

/// <summary>Persistence contract for <see cref="OtpCode"/>.</summary>
public interface IOtpCodeRepository
{
    /// <summary>Returns the current non-expired, non-used OTP for <paramref name="userId"/> and <paramref name="purpose"/>, or <see langword="null"/> if none exists.</summary>
    Task<OtpCode?> GetActiveByUserAndPurposeAsync(Guid userId, OtpPurpose purpose, CancellationToken ct = default);

    /// <summary>Persists a newly generated OTP code.</summary>
    Task AddAsync(OtpCode code, CancellationToken ct = default);

    /// <summary>Persists state changes to an existing OTP code (e.g., marking it used or incrementing attempts).</summary>
    Task UpdateAsync(OtpCode code, CancellationToken ct = default);

    /// <summary>Marks all outstanding OTP codes for <paramref name="userId"/> and <paramref name="purpose"/> as invalid.</summary>
    Task InvalidateAllForUserAsync(Guid userId, OtpPurpose purpose, CancellationToken ct = default);

    /// <summary>Deletes all OTP codes whose expiry has passed; intended for periodic background cleanup.</summary>
    Task ExpireStaleAsync(CancellationToken ct = default);
}
