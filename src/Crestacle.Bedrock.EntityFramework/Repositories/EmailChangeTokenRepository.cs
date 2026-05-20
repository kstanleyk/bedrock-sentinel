using Crestacle.Bedrock.Core.Entities;
using Crestacle.Bedrock.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Crestacle.Bedrock.EntityFramework.Repositories;

internal sealed class EmailChangeTokenRepository : IEmailChangeTokenRepository
{
    private readonly BedrockContext _context;

    public EmailChangeTokenRepository(BedrockContext context) => _context = context;

    public async Task AddAsync(EmailChangeToken token, CancellationToken ct = default)
        => await _context.EmailChangeTokens.AddAsync(token, ct);

    public async Task<EmailChangeToken?> GetByHashAsync(string tokenHash, CancellationToken ct = default)
        => await _context.EmailChangeTokens.AsNoTracking()
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

    public Task UpdateAsync(EmailChangeToken token, CancellationToken ct = default)
    {
        var tracked = _context.ChangeTracker.Entries<EmailChangeToken>()
            .FirstOrDefault(e => e.Entity.Id == token.Id);
        if (tracked is not null)
            tracked.CurrentValues.SetValues(token);
        else
            _context.EmailChangeTokens.Update(token);
        return Task.CompletedTask;
    }

    public async Task InvalidateAllForUserAsync(Guid userId, CancellationToken ct = default)
        => await _context.EmailChangeTokens
            .Where(t => t.UserId == userId && t.UsedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.UsedAt, DateTime.UtcNow), ct);
}
