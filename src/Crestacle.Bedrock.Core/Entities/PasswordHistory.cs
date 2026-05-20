namespace Crestacle.Bedrock.Core.Entities;

/// <summary>
/// Stores past password hashes per user to enforce the configured reuse-prevention policy.
/// Never updated or deleted within the configured history depth.
/// </summary>
public sealed class PasswordHistory
{
    private PasswordHistory() { }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }

    /// <summary>Argon2id hash of the historical password; max 512 chars.</summary>
    public string PasswordHash { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }
    public string? TenantId { get; private set; }

    // -------------------------------------------------------------------------
    // Factory
    // -------------------------------------------------------------------------

    public static PasswordHistory Create(Guid userId, string passwordHash, string? tenantId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        return new PasswordHistory
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PasswordHash = passwordHash,
            TenantId = tenantId,
            CreatedAt = DateTime.UtcNow
        };
    }
}
