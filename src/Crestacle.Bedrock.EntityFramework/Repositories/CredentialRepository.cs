using Crestacle.Bedrock.Core.DTOs;
using Crestacle.Bedrock.Core.Entities;
using Crestacle.Bedrock.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Crestacle.Bedrock.EntityFramework.Repositories;

internal sealed class CredentialRepository : ICredentialRepository
{
    private readonly BedrockContext _context;

    public CredentialRepository(BedrockContext context) => _context = context;

    public async Task<UserCredential?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await _context.UserCredentials.AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId, ct);

    public async Task<UserCredential?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await _context.UserCredentials.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Email == email, ct);

    public async Task AddAsync(UserCredential credential, CancellationToken ct = default)
        => await _context.UserCredentials.AddAsync(credential, ct);

    public Task UpdateAsync(UserCredential credential, CancellationToken ct = default)
    {
        var tracked = _context.ChangeTracker.Entries<UserCredential>()
            .FirstOrDefault(e => e.Entity.Id == credential.Id);
        if (tracked is not null)
            tracked.CurrentValues.SetValues(credential);
        else
            _context.UserCredentials.Update(credential);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
        => await _context.UserCredentials.AnyAsync(c => c.Email == email, ct);

    public async Task<PagedResult<CredentialSummary>> GetPagedAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = _context.UserCredentials.AsNoTracking();
        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CredentialSummary(
                c.UserId, c.Email, c.Status, c.EmailConfirmed, c.MfaEnabled,
                c.LockoutEnd, c.CreatedAt))
            .ToListAsync(ct);
        return new PagedResult<CredentialSummary>(items, totalCount, page, pageSize);
    }
}
