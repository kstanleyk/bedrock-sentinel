namespace Crestacle.Sentinel.Core.Entities;

public sealed class User : Entity<Guid>
{
    public string  IdentityId { get; private set; } = string.Empty;
    public string  Email      { get; private set; } = string.Empty;
    public string? FullName   { get; private set; }
    public string? Phone      { get; private set; }

    /// <summary>Tenant this user belongs to. Null in single-tenant deployments.</summary>
    public string? TenantId   { get; private set; }

    public DateTime CreatedOn { get; private set; }

    public ICollection<UserRole> UserRoles { get; private set; } = [];

    private User() { }

    public static User Create(
        string  identityId,
        string  email,
        string? fullName = null,
        string? phone    = null,
        string? tenantId = null)
        => new()
        {
            Id         = Guid.NewGuid(),
            IdentityId = identityId,
            Email      = email,
            FullName   = fullName,
            Phone      = phone,
            TenantId   = tenantId,
            CreatedOn  = DateTime.UtcNow,
        };
}
