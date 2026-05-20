using Crestacle.Bedrock.Core.Entities;
using Crestacle.Bedrock.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Crestacle.Bedrock.EntityFramework.Repositories;

internal sealed class SessionRepository : ISessionRepository
{
    private readonly BedrockContext _context;

    public SessionRepository(BedrockContext context) => _context = context;

    public async Task<Session?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Sessions.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<Session?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default)
        => await _context.Sessions.AsNoTracking()
            .FirstOrDefaultAsync(s => s.TokenHash == tokenHash, ct);

    public async Task<IReadOnlyList<Session>> GetActiveByUserAsync(
        Guid userId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await _context.Sessions.AsNoTracking()
            .Where(s => s.UserId == userId && s.RevokedAt == null && s.ExpiresAt > now)
            .OrderByDescending(s => s.LastActivityAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Session session, CancellationToken ct = default)
        => await _context.Sessions.AddAsync(session, ct);

    public Task UpdateAsync(Session session, CancellationToken ct = default)
    {
        var tracked = _context.ChangeTracker.Entries<Session>()
            .FirstOrDefault(e => e.Entity.Id == session.Id);
        if (tracked is not null)
            tracked.CurrentValues.SetValues(session);
        else
            _context.Sessions.Update(session);
        return Task.CompletedTask;
    }

    public async Task RevokeAllForUserAsync(Guid userId, string byIp, CancellationToken ct = default)
        => await _context.Sessions
            .Where(s => s.UserId == userId && s.RevokedAt == null)
            .ExecuteUpdateAsync(calls => calls
                .SetProperty(s => s.RevokedAt, DateTime.UtcNow)
                .SetProperty(s => s.RevokedByIp, byIp), ct);

    public async Task<int> CountActiveForUserAsync(Guid userId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await _context.Sessions
            .CountAsync(s => s.UserId == userId && s.RevokedAt == null && s.ExpiresAt > now, ct);
    }

    public async Task<Session?> GetOldestActiveForUserAsync(Guid userId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await _context.Sessions.AsNoTracking()
            .Where(s => s.UserId == userId && s.RevokedAt == null && s.ExpiresAt > now)
            .OrderBy(s => s.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }
}
