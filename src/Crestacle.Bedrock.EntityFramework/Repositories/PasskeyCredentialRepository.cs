using Crestacle.Bedrock.Core.Entities;
using Crestacle.Bedrock.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Crestacle.Bedrock.EntityFramework.Repositories;

internal sealed class PasskeyCredentialRepository : IPasskeyCredentialRepository
{
    private readonly BedrockContext _context;

    public PasskeyCredentialRepository(BedrockContext context) => _context = context;

    public async Task AddAsync(PasskeyCredential credential, CancellationToken ct = default)
        => await _context.PasskeyCredentials.AddAsync(credential, ct);

    public async Task<PasskeyCredential?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.PasskeyCredentials.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<PasskeyCredential?> GetByCredentialIdAsync(byte[] credentialId, CancellationToken ct = default)
        => await _context.PasskeyCredentials.AsNoTracking()
            .FirstOrDefaultAsync(c => c.CredentialId == credentialId, ct);

    public async Task<IReadOnlyList<PasskeyCredential>> GetForUserAsync(Guid userId, CancellationToken ct = default)
        => await _context.PasskeyCredentials.AsNoTracking()
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);

    public Task UpdateAsync(PasskeyCredential credential, CancellationToken ct = default)
    {
        var tracked = _context.ChangeTracker.Entries<PasskeyCredential>()
            .FirstOrDefault(e => e.Entity.Id == credential.Id);
        if (tracked is not null)
            tracked.CurrentValues.SetValues(credential);
        else
            _context.PasskeyCredentials.Update(credential);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(PasskeyCredential credential, CancellationToken ct = default)
    {
        var tracked = _context.ChangeTracker.Entries<PasskeyCredential>()
            .FirstOrDefault(e => e.Entity.Id == credential.Id);
        if (tracked is not null)
            tracked.State = Microsoft.EntityFrameworkCore.EntityState.Deleted;
        else
            _context.PasskeyCredentials.Remove(credential);
        return Task.CompletedTask;
    }
}
