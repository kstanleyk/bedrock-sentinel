namespace Crestacle.Bedrock.Core.Entities;

public sealed class ExternalIdentity
{
    private ExternalIdentity() { }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Provider { get; private set; } = string.Empty;
    public string ProviderUserId { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public string? TenantId { get; private set; }

    public static ExternalIdentity Create(
        Guid userId,
        string provider,
        string providerUserId,
        string? tenantId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(provider);
        ArgumentException.ThrowIfNullOrWhiteSpace(providerUserId);

        return new ExternalIdentity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Provider = provider,
            ProviderUserId = providerUserId,
            CreatedAt = DateTime.UtcNow,
            TenantId = tenantId
        };
    }
}
