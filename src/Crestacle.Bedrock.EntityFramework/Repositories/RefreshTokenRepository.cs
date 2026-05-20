using Crestacle.Bedrock.Core.Entities;
using Crestacle.Bedrock.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Crestacle.Bedrock.EntityFramework.Repositories;

internal sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly BedrockContext _context;

    public RefreshTokenRepository(BedrockContext context) => _context = context;

    public async Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken ct = default)
        => await _context.RefreshTokens.AsNoTracking()
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

    public async Task AddAsync(RefreshToken token, CancellationToken ct = default)
        => await _context.RefreshTokens.AddAsync(token, ct);

    public Task UpdateAsync(RefreshToken token, CancellationToken ct = default)
    {
        var tracked = _context.ChangeTracker.Entries<RefreshToken>()
            .FirstOrDefault(e => e.Entity.Id == token.Id);
        if (tracked is not null)
            tracked.CurrentValues.SetValues(token);
        else
            _context.RefreshTokens.Update(token);
        return Task.CompletedTask;
    }

    public async Task RevokeAllForUserAsync(Guid userId, string byIp, CancellationToken ct = default)
        => await _context.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s
                .SetProperty(t => t.RevokedAt, DateTime.UtcNow)
                .SetProperty(t => t.RevokedByIp, byIp), ct);

    public async Task<int> CountActiveForUserAsync(Guid userId, CancellationToken ct = default)
        => await _context.RefreshTokens
            .CountAsync(t => t.UserId == userId && t.RevokedAt == null && t.ExpiresAt > DateTime.UtcNow, ct);
}
