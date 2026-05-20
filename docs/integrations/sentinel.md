# Sentinel RBAC integration

Sentinel is a role-based access control (RBAC) library that integrates with Bedrock to add permissions and roles to your application. It is distributed as a separate set of packages and connected to Bedrock via the `Crestacle.Bedrock.Sentinel` bridge package.

---

## Packages

```bash
dotnet add package Crestacle.Bedrock.Sentinel
```

This meta-package includes `Crestacle.Sentinel.Core`, `Crestacle.Sentinel.AspNetCore`, and `Crestacle.Sentinel.EntityFramework`.

---

## Database setup

Sentinel stores roles and permissions in its own schema. You can share a single `DbContext` with Bedrock, or use separate contexts.

### Shared context (recommended for most applications)

Have your `DbContext` inherit `BedrockContext` and also implement `IAuthDbContext`:

```csharp
public class AppDbContext : BedrockContext, IAuthDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Sentinel tables
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<UserPermission> UserPermissions { get; set; } = null!;
    public DbSet<PermissionConflict> PermissionConflicts { get; set; } = null!;
    public DbSet<PendingAssignment> PendingAssignments { get; set; } = null!;
    public DbSet<SentinelAuditEntry> SentinelAuditEntries { get; set; } = null!;
    public DbSet<SentinelUser> SentinelUsers { get; set; } = null!;
}
```

### Separate context

If you prefer Sentinel data in its own database or context:

```csharp
public class SentinelDbContext : DbContext, IAuthDbContext
{
    public SentinelDbContext(DbContextOptions<SentinelDbContext> options)
        : base(options) { }

    public DbSet<Role> Roles { get; set; } = null!;
    // ... remaining IAuthDbContext DbSets
}
```

---

## Registration

### Combined registration (simplest)

```csharp
builder.Services
    .AddBedrockWithSentinel<AppDbContext, AppDbContext>(
        bedrock =>
        {
            bedrock.Jwt.Issuer = "https://yourapp.com";
            bedrock.Jwt.Audience = "yourapp";
            bedrock.Jwt.SigningKey = builder.Configuration["Jwt:SigningKey"];
            bedrock.Email.FrontendBaseUrl = "https://yourapp.com";
        },
        sentinel =>
        {
            sentinel.PermissionCacheTtl = TimeSpan.FromMinutes(5);
        })
    .WithPermissionClaims()  // Embeds permissions into JWTs
    .WithEmailSender<YourEmailSender>()
    .AddBedrockControllers();
```

When both context type parameters are the same (`AppDbContext, AppDbContext`), a single context is used for both Bedrock and Sentinel tables.

### Separate registration

```csharp
builder.Services
    .AddBedrockEntityFramework<AppDbContext>()
    .AddBedrockAspNetCore(opts => { ... })
    .AddSentinel<SentinelDbContext>(opts =>
    {
        opts.PermissionCacheTtl = TimeSpan.FromMinutes(10);
    })
    .WithPermissionClaims()
    .AddBedrockControllers();
```

---

## Using permissions in your API

Apply the `[MustHavePermission]` attribute to controllers or actions:

```csharp
[MustHavePermission("orders:read")]
[HttpGet("orders")]
public IActionResult GetOrders() { ... }

[MustHavePermission("orders:write")]
[HttpPost("orders")]
public IActionResult CreateOrder() { ... }
```

The attribute generates a dynamic ASP.NET Core policy for each permission string. You do not need to register policies manually.

---

## Embedding permissions in JWTs

When `.WithPermissionClaims()` is called, Sentinel's `SentinelClaimsEnricher` is registered as the `IBedrockClaimsEnricher`. It adds the user's resolved permission set as claims inside the JWT, so downstream services can authorise requests without a database call.

Enable claims caching to avoid re-resolving permissions on every request:

```csharp
.WithPermissionClaims()
.WithClaimsEnricherCache(TimeSpan.FromMinutes(5))
```

Permission claims are added under the `permissions` claim key as an array:

```json
{
  "sub": "...",
  "permissions": ["orders:read", "orders:write", "products:read"]
}
```

---

## SentinelOptions

| Property | Type | Default | Notes |
|---|---|---|---|
| `PermissionCacheTtl` | `TimeSpan` | `5 minutes` | How long resolved permission sets are cached in-process. |

For multi-pod deployments, replace `MemoryPermissionCache` with a distributed implementation.

---

## Migrations

Sentinel entity configurations are applied through `IAuthDbContext`. Run migrations as normal after adding the `IAuthDbContext` DbSets to your context:

```bash
dotnet ef migrations add AddSentinel --context AppDbContext
dotnet ef database update --context AppDbContext
```
