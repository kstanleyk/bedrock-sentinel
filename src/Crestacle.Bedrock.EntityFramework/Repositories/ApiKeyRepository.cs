using Crestacle.Bedrock.Core.Entities;
using Crestacle.Bedrock.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Crestacle.Bedrock.EntityFramework.Repositories;

internal sealed class ApiKeyRepository : IApiKeyRepository
{
    private readonly BedrockContext _context;

    public ApiKeyRepository(BedrockContext context) => _context = context;

    public async Task AddAsync(ApiKey key, CancellationToken ct = default)
        => await _context.ApiKeys.AddAsync(key, ct);

    public async Task<ApiKey?> GetByHashAsync(string keyHash, CancellationToken ct = default)
        => await _context.ApiKeys.AsNoTracking()
            .FirstOrDefaultAsync(k => k.KeyHash == keyHash, ct);

    public async Task<IReadOnlyList<ApiKey>> GetForUserAsync(Guid userId, CancellationToken ct = default)
        => await _context.ApiKeys.AsNoTracking()
            .Where(k => k.UserId == userId)
            .OrderByDescending(k => k.CreatedAt)
            .ToListAsync(ct);

    public async Task UpdateLastUsedAsync(Guid keyId, CancellationToken ct = default)
        => await _context.ApiKeys
            .Where(k => k.Id == keyId)
            .ExecuteUpdateAsync(s => s.SetProperty(k => k.LastUsedAt, DateTime.UtcNow), ct);

    public async Task RevokeAsync(Guid keyId, Guid userId, CancellationToken ct = default)
        => await _context.ApiKeys
            .Where(k => k.Id == keyId && k.UserId == userId && k.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(k => k.RevokedAt, DateTime.UtcNow), ct);
}
