using Crestacle.Bedrock.Core.Entities;
using Crestacle.Bedrock.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Crestacle.Bedrock.EntityFramework.Repositories;

internal sealed class PasswordHistoryRepository : IPasswordHistoryRepository
{
    private readonly BedrockContext _context;

    public PasswordHistoryRepository(BedrockContext context) => _context = context;

    public async Task<IReadOnlyList<PasswordHistory>> GetRecentByUserAsync(
        Guid userId, int depth, CancellationToken ct = default)
        => await _context.PasswordHistories.AsNoTracking()
            .Where(ph => ph.UserId == userId)
            .OrderByDescending(ph => ph.CreatedAt)
            .Take(depth)
            .ToListAsync(ct);

    public async Task AddAsync(PasswordHistory entry, CancellationToken ct = default)
        => await _context.PasswordHistories.AddAsync(entry, ct);

    public async Task PruneAsync(Guid userId, int keepCount, CancellationToken ct = default)
    {
        var idsToKeep = await _context.PasswordHistories
            .Where(ph => ph.UserId == userId)
            .OrderByDescending(ph => ph.CreatedAt)
            .Take(keepCount)
            .Select(ph => ph.Id)
            .ToListAsync(ct);

        await _context.PasswordHistories
            .Where(ph => ph.UserId == userId && !idsToKeep.Contains(ph.Id))
            .ExecuteDeleteAsync(ct);
    }
}
