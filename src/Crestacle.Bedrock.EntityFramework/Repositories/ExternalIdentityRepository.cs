using Crestacle.Bedrock.Core.Entities;
using Crestacle.Bedrock.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Crestacle.Bedrock.EntityFramework.Repositories;

internal sealed class ExternalIdentityRepository : IExternalIdentityRepository
{
    private readonly BedrockContext _context;

    public ExternalIdentityRepository(BedrockContext context) => _context = context;

    public async Task AddAsync(ExternalIdentity identity, CancellationToken ct = default)
        => await _context.ExternalIdentities.AddAsync(identity, ct);

    public async Task<ExternalIdentity?> GetByProviderAsync(
        string provider,
        string providerUserId,
        CancellationToken ct = default)
        => await _context.ExternalIdentities.AsNoTracking()
            .FirstOrDefaultAsync(
                e => e.Provider == provider && e.ProviderUserId == providerUserId, ct);

    public async Task<IReadOnlyList<ExternalIdentity>> GetForUserAsync(
        Guid userId,
        CancellationToken ct = default)
        => await _context.ExternalIdentities.AsNoTracking()
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(ct);

    public Task DeleteAsync(ExternalIdentity identity, CancellationToken ct = default)
    {
        var tracked = _context.ChangeTracker.Entries<ExternalIdentity>()
            .FirstOrDefault(e => e.Entity.Id == identity.Id);
        if (tracked is not null)
            tracked.State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
        else
            _context.ExternalIdentities.Remove(identity);
        return Task.CompletedTask;
    }
}
