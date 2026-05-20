using Crestacle.Bedrock.Core.Entities;

namespace Crestacle.Bedrock.Core.Interfaces.Repositories;

/// <summary>Persistence contract for <see cref="KnownDevice"/>.</summary>
public interface IKnownDeviceRepository
{
    /// <summary>Returns the device record matching <paramref name="fingerprintHash"/> for <paramref name="userId"/>, or <see langword="null"/> if the device is unrecognised.</summary>
    Task<KnownDevice?> GetByFingerprintAsync(Guid userId, string fingerprintHash, CancellationToken ct = default);

    /// <summary>Returns all known devices registered for <paramref name="userId"/>.</summary>
    Task<IReadOnlyList<KnownDevice>> GetByUserAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Persists a newly recognised device.</summary>
    Task AddAsync(KnownDevice device, CancellationToken ct = default);

    /// <summary>Persists state changes to an existing known-device record (e.g., updating the last-seen timestamp).</summary>
    Task UpdateAsync(KnownDevice device, CancellationToken ct = default);
}
