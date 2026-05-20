using Crestacle.Bedrock.Core.Entities;
using Crestacle.Bedrock.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Crestacle.Bedrock.EntityFramework.Repositories;

internal sealed class InvitationRepository : IInvitationRepository
{
    private readonly BedrockContext _context;

    public InvitationRepository(BedrockContext context) => _context = context;

    public async Task AddAsync(Invitation invitation, CancellationToken ct = default)
        => await _context.Invitations.AddAsync(invitation, ct);

    public async Task<Invitation?> GetByHashAsync(string tokenHash, CancellationToken ct = default)
        => await _context.Invitations.AsNoTracking()
            .FirstOrDefaultAsync(i => i.TokenHash == tokenHash, ct);

    public Task UpdateAsync(Invitation invitation, CancellationToken ct = default)
    {
        var tracked = _context.ChangeTracker.Entries<Invitation>()
            .FirstOrDefault(e => e.Entity.Id == invitation.Id);
        if (tracked is not null)
            tracked.CurrentValues.SetValues(invitation);
        else
            _context.Invitations.Update(invitation);
        return Task.CompletedTask;
    }
}
