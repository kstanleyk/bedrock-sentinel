namespace Crestacle.Bedrock.Core.Entities;

public sealed class PasskeyCredential
{
    private PasskeyCredential() { }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public byte[] CredentialId { get; private set; } = [];
    public byte[] PublicKeyCose { get; private set; } = [];
    public long SignCount { get; private set; }
    public string? Transports { get; private set; }
    public bool IsBackedUp { get; private set; }
    public string? FriendlyName { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastUsedAt { get; private set; }
    public string? TenantId { get; private set; }

    public static PasskeyCredential Create(
        Guid userId,
        byte[] credentialId,
        byte[] publicKeyCose,
        long signCount,
        string? transports = null,
        bool isBackedUp = false,
        string? friendlyName = null,
        string? tenantId = null)
    {
        ArgumentNullException.ThrowIfNull(credentialId);
        ArgumentNullException.ThrowIfNull(publicKeyCose);

        return new PasskeyCredential
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CredentialId = credentialId,
            PublicKeyCose = publicKeyCose,
            SignCount = signCount,
            Transports = transports,
            IsBackedUp = isBackedUp,
            FriendlyName = friendlyName,
            CreatedAt = DateTime.UtcNow,
            TenantId = tenantId
        };
    }

    public void UpdateUsage(long newSignCount)
    {
        SignCount = newSignCount;
        LastUsedAt = DateTime.UtcNow;
    }
}
