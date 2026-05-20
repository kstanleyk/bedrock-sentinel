using Crestacle.Bedrock.Core.Entities;
using Crestacle.Bedrock.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Crestacle.Bedrock.EntityFramework.Repositories;

internal sealed class EmailVerificationTokenRepository : IEmailVerificationTokenRepository
{
    private readonly BedrockContext _context;

    public EmailVerificationTokenRepository(BedrockContext context) => _context = context;

    public async Task<EmailVerificationToken?> GetByHashAsync(
        string tokenHash, CancellationToken ct = default)
        => await _context.EmailVerificationTokens.AsNoTracking()
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

    public async Task AddAsync(EmailVerificationToken token, CancellationToken ct = default)
        => await _context.EmailVerificationTokens.AddAsync(token, ct);

    public Task UpdateAsync(EmailVerificationToken token, CancellationToken ct = default)
    {
        var tracked = _context.ChangeTracker.Entries<EmailVerificationToken>()
            .FirstOrDefault(e => e.Entity.Id == token.Id);
        if (tracked is not null)
            tracked.CurrentValues.SetValues(token);
        else
            _context.EmailVerificationTokens.Update(token);
        return Task.CompletedTask;
    }

    public async Task InvalidateAllForUserAsync(Guid userId, CancellationToken ct = default)
        => await _context.EmailVerificationTokens
            .Where(t => t.UserId == userId && t.UsedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.UsedAt, DateTime.UtcNow), ct);
}
