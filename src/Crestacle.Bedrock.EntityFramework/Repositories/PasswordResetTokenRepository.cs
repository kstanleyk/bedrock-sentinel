using Crestacle.Bedrock.Core.Entities;
using Crestacle.Bedrock.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Crestacle.Bedrock.EntityFramework.Repositories;

internal sealed class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly BedrockContext _context;

    public PasswordResetTokenRepository(BedrockContext context) => _context = context;

    public async Task<PasswordResetToken?> GetByHashAsync(
        string tokenHash, CancellationToken ct = default)
        => await _context.PasswordResetTokens.AsNoTracking()
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

    public async Task AddAsync(PasswordResetToken token, CancellationToken ct = default)
        => await _context.PasswordResetTokens.AddAsync(token, ct);

    public Task UpdateAsync(PasswordResetToken token, CancellationToken ct = default)
    {
        var tracked = _context.ChangeTracker.Entries<PasswordResetToken>()
            .FirstOrDefault(e => e.Entity.Id == token.Id);
        if (tracked is not null)
            tracked.CurrentValues.SetValues(token);
        else
            _context.PasswordResetTokens.Update(token);
        return Task.CompletedTask;
    }

    public async Task InvalidateAllForUserAsync(Guid userId, CancellationToken ct = default)
        => await _context.PasswordResetTokens
            .Where(t => t.UserId == userId && t.UsedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.UsedAt, DateTime.UtcNow), ct);
}
