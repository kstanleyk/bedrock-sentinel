using Crestacle.Bedrock.Core.Entities;
using Crestacle.Bedrock.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Crestacle.Bedrock.EntityFramework.Repositories;

internal sealed class ConsentRepository : IConsentRepository
{
    private readonly BedrockContext _context;

    public ConsentRepository(BedrockContext context) => _context = context;

    public async Task AddAsync(ConsentRecord record, CancellationToken ct = default)
        => await _context.ConsentRecords.AddAsync(record, ct);

    public async Task<IReadOnlyList<ConsentRecord>> GetForUserAsync(Guid userId, CancellationToken ct = default)
        => await _context.ConsentRecords
            .AsNoTracking()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.AcceptedAt)
            .ToListAsync(ct);
}
