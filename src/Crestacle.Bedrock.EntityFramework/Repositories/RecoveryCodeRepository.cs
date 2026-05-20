using Crestacle.Bedrock.Core.Entities;
using Crestacle.Bedrock.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Crestacle.Bedrock.EntityFramework.Repositories;

internal sealed class RecoveryCodeRepository : IRecoveryCodeRepository
{
    private readonly BedrockContext _context;

    public RecoveryCodeRepository(BedrockContext context) => _context = context;

    public async Task<IReadOnlyList<RecoveryCode>> GetAvailableByUserAsync(
        Guid userId, CancellationToken ct = default)
        => await _context.RecoveryCodes.AsNoTracking()
            .Where(rc => rc.UserId == userId && rc.UsedAt == null)
            .ToListAsync(ct);

    public async Task<RecoveryCode?> GetByHashAsync(
        Guid userId, string codeHash, CancellationToken ct = default)
        => await _context.RecoveryCodes.AsNoTracking()
            .FirstOrDefaultAsync(rc => rc.UserId == userId && rc.CodeHash == codeHash, ct);

    public async Task AddRangeAsync(IEnumerable<RecoveryCode> codes, CancellationToken ct = default)
        => await _context.RecoveryCodes.AddRangeAsync(codes, ct);

    public Task UpdateAsync(RecoveryCode code, CancellationToken ct = default)
    {
        var tracked = _context.ChangeTracker.Entries<RecoveryCode>()
            .FirstOrDefault(e => e.Entity.Id == code.Id);
        if (tracked is not null)
            tracked.CurrentValues.SetValues(code);
        else
            _context.RecoveryCodes.Update(code);
        return Task.CompletedTask;
    }

    public async Task InvalidateAllForUserAsync(Guid userId, CancellationToken ct = default)
        => await _context.RecoveryCodes
            .Where(rc => rc.UserId == userId && rc.UsedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(rc => rc.UsedAt, DateTime.UtcNow), ct);

    public async Task<int> CountAvailableAsync(Guid userId, CancellationToken ct = default)
        => await _context.RecoveryCodes
            .CountAsync(rc => rc.UserId == userId && rc.UsedAt == null, ct);
}
