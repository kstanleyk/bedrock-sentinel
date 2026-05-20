using Crestacle.Bedrock.Core.Entities;
using Crestacle.Bedrock.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Crestacle.Bedrock.EntityFramework.Repositories;

internal sealed class KnownDeviceRepository : IKnownDeviceRepository
{
    private readonly BedrockContext _context;

    public KnownDeviceRepository(BedrockContext context) => _context = context;

    public async Task<KnownDevice?> GetByFingerprintAsync(
        Guid userId, string fingerprintHash, CancellationToken ct = default)
        => await _context.KnownDevices.AsNoTracking()
            .FirstOrDefaultAsync(d => d.UserId == userId && d.FingerprintHash == fingerprintHash, ct);

    public async Task<IReadOnlyList<KnownDevice>> GetByUserAsync(
        Guid userId, CancellationToken ct = default)
        => await _context.KnownDevices.AsNoTracking()
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.LastSeenAt)
            .ToListAsync(ct);

    public async Task AddAsync(KnownDevice device, CancellationToken ct = default)
        => await _context.KnownDevices.AddAsync(device, ct);

    public Task UpdateAsync(KnownDevice device, CancellationToken ct = default)
    {
        var tracked = _context.ChangeTracker.Entries<KnownDevice>()
            .FirstOrDefault(e => e.Entity.Id == device.Id);
        if (tracked is not null)
            tracked.CurrentValues.SetValues(device);
        else
            _context.KnownDevices.Update(device);
        return Task.CompletedTask;
    }
}
