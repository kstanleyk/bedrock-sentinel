namespace Crestacle.Bedrock.Core.Interfaces.Repositories;

/// <summary>
/// Thin wrapper over the underlying <c>DbContext.SaveChangesAsync</c>.
/// All repositories operate on the same <c>DbContext</c> instance per request.
/// Callers invoke this once per command; repositories never call save themselves.
/// </summary>
public interface IBedrockUnitOfWork
{
    /// <summary>Flushes all pending changes to the database and returns the number of affected rows.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
