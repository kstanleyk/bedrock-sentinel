using Crestacle.Bedrock.Core.Entities;
using Crestacle.Bedrock.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Crestacle.Bedrock.EntityFramework.Repositories;

internal sealed class MfaChallengeRepository : IMfaChallengeRepository
{
    private readonly BedrockContext _context;

    public MfaChallengeRepository(BedrockContext context) => _context = context;

    public async Task<MfaChallenge?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.MfaChallenges.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task AddAsync(MfaChallenge challenge, CancellationToken ct = default)
        => await _context.MfaChallenges.AddAsync(challenge, ct);

    public Task UpdateAsync(MfaChallenge challenge, CancellationToken ct = default)
    {
        var tracked = _context.ChangeTracker.Entries<MfaChallenge>()
            .FirstOrDefault(e => e.Entity.Id == challenge.Id);
        if (tracked is not null)
            tracked.CurrentValues.SetValues(challenge);
        else
            _context.MfaChallenges.Update(challenge);
        return Task.CompletedTask;
    }

    public async Task ExpireStaleAsync(CancellationToken ct = default)
        => await _context.MfaChallenges
            .Where(c => c.ExpiresAt < DateTime.UtcNow)
            .ExecuteDeleteAsync(ct);
}
