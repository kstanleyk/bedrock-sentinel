using Crestacle.Bedrock.Core.Entities;
using Crestacle.Bedrock.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Crestacle.Bedrock.EntityFramework.Repositories;

internal sealed class StepUpChallengeRepository : IStepUpChallengeRepository
{
    private readonly BedrockContext _context;

    public StepUpChallengeRepository(BedrockContext context) => _context = context;

    public async Task<StepUpChallenge?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.StepUpChallenges.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task AddAsync(StepUpChallenge challenge, CancellationToken ct = default)
        => await _context.StepUpChallenges.AddAsync(challenge, ct);

    public Task UpdateAsync(StepUpChallenge challenge, CancellationToken ct = default)
    {
        var tracked = _context.ChangeTracker.Entries<StepUpChallenge>()
            .FirstOrDefault(e => e.Entity.Id == challenge.Id);
        if (tracked is not null)
            tracked.CurrentValues.SetValues(challenge);
        else
            _context.StepUpChallenges.Update(challenge);
        return Task.CompletedTask;
    }

    public async Task ExpireStaleAsync(CancellationToken ct = default)
        => await _context.StepUpChallenges
            .Where(c => c.ExpiresAt < DateTime.UtcNow)
            .ExecuteDeleteAsync(ct);
}
