using Crestacle.Bedrock.Core.DTOs;
using Crestacle.Bedrock.Core.Entities;
using Crestacle.Bedrock.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Crestacle.Bedrock.EntityFramework.Repositories;

internal sealed class AuditRepository : IAuditRepository
{
    private readonly BedrockContext _context;

    public AuditRepository(BedrockContext context) => _context = context;

    public async Task AddAsync(AuditEntry entry, CancellationToken ct = default)
        => await _context.AuditEntries.AddAsync(entry, ct);

    public async Task<AuditQueryResult> QueryAsync(AuditQueryFilter filter, CancellationToken ct = default)
    {
        var query = _context.AuditEntries.AsNoTracking().AsQueryable();

        if (filter.UserId.HasValue)
            query = query.Where(e => e.UserId == filter.UserId.Value);

        if (filter.EventType.HasValue)
            query = query.Where(e => e.EventType == filter.EventType.Value);

        if (filter.From.HasValue)
            query = query.Where(e => e.OccurredAt >= filter.From.Value);

        if (filter.To.HasValue)
            query = query.Where(e => e.OccurredAt <= filter.To.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(e => e.OccurredAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        return new AuditQueryResult(items, totalCount);
    }
}
