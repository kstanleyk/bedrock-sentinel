using Crestacle.Bedrock.Core.Entities;
using Crestacle.Bedrock.Core.Enumerations;
using Crestacle.Bedrock.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Crestacle.Bedrock.EntityFramework.Repositories;

internal sealed class OtpCodeRepository : IOtpCodeRepository
{
    private readonly BedrockContext _context;

    public OtpCodeRepository(BedrockContext context) => _context = context;

    public async Task<OtpCode?> GetActiveByUserAndPurposeAsync(
        Guid userId, OtpPurpose purpose, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await _context.OtpCodes.AsNoTracking()
            .FirstOrDefaultAsync(c =>
                c.UserId == userId &&
                c.Purpose == purpose &&
                c.UsedAt == null &&
                c.InvalidatedAt == null &&
                c.ExpiresAt > now, ct);
    }

    public async Task AddAsync(OtpCode code, CancellationToken ct = default)
        => await _context.OtpCodes.AddAsync(code, ct);

    public Task UpdateAsync(OtpCode code, CancellationToken ct = default)
    {
        var tracked = _context.ChangeTracker.Entries<OtpCode>()
            .FirstOrDefault(e => e.Entity.Id == code.Id);
        if (tracked is not null)
            tracked.CurrentValues.SetValues(code);
        else
            _context.OtpCodes.Update(code);
        return Task.CompletedTask;
    }

    public async Task InvalidateAllForUserAsync(Guid userId, OtpPurpose purpose, CancellationToken ct = default)
        => await _context.OtpCodes
            .Where(c => c.UserId == userId && c.Purpose == purpose &&
                        c.UsedAt == null && c.InvalidatedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.InvalidatedAt, DateTime.UtcNow), ct);

    public async Task ExpireStaleAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        await _context.OtpCodes
            .Where(c => c.ExpiresAt < now && c.UsedAt == null && c.InvalidatedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.InvalidatedAt, now), ct);
    }
}
