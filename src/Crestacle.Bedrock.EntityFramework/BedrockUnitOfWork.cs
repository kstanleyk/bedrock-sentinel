using Crestacle.Bedrock.Core.Exceptions;
using Crestacle.Bedrock.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Crestacle.Bedrock.EntityFramework;

public sealed class BedrockUnitOfWork : IBedrockUnitOfWork
{
    private readonly BedrockContext _context;

    public BedrockUnitOfWork(BedrockContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        try
        {
            return await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new BedrockConcurrencyException(
                "A concurrency conflict occurred. Please retry the operation.", ex);
        }
    }
}
