using Crestacle.Bedrock.Core.Entities;
using Crestacle.Bedrock.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Crestacle.Bedrock.EntityFramework.Repositories;

internal sealed class MagicLinkTokenRepository : IMagicLinkTokenRepository
{
    private readonly BedrockContext _context;

    public MagicLinkTokenRepository(BedrockContext context) => _context = context;

    public async Task<MagicLinkToken?> GetByHashAsync(
        string tokenHash, CancellationToken ct = default)
        => await _context.MagicLinkTokens.AsNoTracking()
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

    public async Task AddAsync(MagicLinkToken token, CancellationToken ct = default)
        => await _context.MagicLinkTokens.AddAsync(token, ct);

    public Task UpdateAsync(MagicLinkToken token, CancellationToken ct = default)
    {
        var tracked = _context.ChangeTracker.Entries<MagicLinkToken>()
            .FirstOrDefault(e => e.Entity.Id == token.Id);
        if (tracked is not null)
            tracked.CurrentValues.SetValues(token);
        else
            _context.MagicLinkTokens.Update(token);
        return Task.CompletedTask;
    }

    public async Task InvalidateAllForUserAsync(Guid userId, CancellationToken ct = default)
        => await _context.MagicLinkTokens
            .Where(t => t.UserId == userId && t.UsedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.UsedAt, DateTime.UtcNow), ct);
}
