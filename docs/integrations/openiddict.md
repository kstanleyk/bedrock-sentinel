# OpenIddict integration

This guide shows how to use [OpenIddict](https://documentation.openiddict.com/) as the token authority alongside Bedrock. Bedrock handles credential management, MFA, sessions, and audit; OpenIddict handles standards-compliant OAuth 2.0 / OpenID Connect token issuance.

This is the recommended path when you need:
- Full OIDC discovery (`.well-known/openid-configuration`)
- Authorization code flow for third-party clients
- Token introspection or reference tokens
- Automatic signing key rotation managed by OpenIddict

If you only need JWT access tokens for your own first-party clients, the built-in `JwtBedrockTokenIssuer` is simpler and sufficient. See [deployment models](../deployment-models.md) for guidance on choosing.

---

## How it fits together

```
Client
  │
  ▼
POST /api/bedrock/auth/login         ← Bedrock verifies credentials, MFA, etc.
  │
  ▼
IBedrockTokenIssuer.IssueAccessTokenAsync()
  │  (internal HTTP call with a trusted assertion)
  ▼
POST /connect/token                  ← OpenIddict issues the signed JWT
  │
  ▼
AccessTokenDescriptor returned to Bedrock's session layer
  │
  ▼
Client receives OpenIddict-issued access token
```

Bedrock's login flow calls `IBedrockTokenIssuer` after credential verification. The implementation makes a server-to-server HTTP call to OpenIddict's token endpoint using a short-lived internal assertion. OpenIddict validates the assertion, builds the claims principal, and issues the signed access token.

---

## Packages

```bash
dotnet add package OpenIddict.AspNetCore
dotnet add package OpenIddict.EntityFrameworkCore
```

---

## 1. DbContext

Add OpenIddict entity support to your `BedrockContext`-derived context by overriding `OnModelCreating`:

```csharp
public class AppDbContext : BedrockContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.UseOpenIddict();   // registers OpenIddict tables
    }
}
```

`modelBuilder.UseOpenIddict()` creates four tables: `OpenIddictApplications`, `OpenIddictAuthorizations`, `OpenIddictScopes`, and `OpenIddictTokens`.

---

## 2. Registration

```csharp
builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("Default"))
        .UseOpenIddict());  // tells EF Core to load OpenIddict entity configurations

builder.Services
    .AddBedrockEntityFramework<AppDbContext>()
    .AddBedrockAspNetCore(opts =>
    {
        opts.Jwt.ExternalTokenIssuer = true;

        // Bedrock still needs a signing key for its internal tokens
        // (MFA challenges, step-up tokens, enrollment tokens).
        opts.Jwt.SigningKey = builder.Configuration["Jwt:InternalSigningKey"];

        opts.Email.FrontendBaseUrl = builder.Configuration["FrontendBaseUrl"]!;
        opts.Mfa.Issuer = "Your App";
    })
    .WithTokenIssuer<OpenIddictBedrockTokenIssuer>()
    .WithEmailSender<YourEmailSender>()
    .AddBedrockControllers();

builder.Services.AddOpenIddict()
    .AddCore(opts =>
    {
        opts.UseEntityFrameworkCore()
            .UseDbContext<AppDbContext>();
    })
    .AddServer(opts =>
    {
        opts.SetIssuer(new Uri(builder.Configuration["OpenIddict:Issuer"]!));
        opts.SetTokenEndpointUris("connect/token");

        // Allow the custom grant Bedrock uses for token exchange
        opts.AllowCustomFlow("urn:bedrock:exchange");

        // Standard flows for other clients (add only what you need)
        opts.AllowClientCredentialsFlow();
        opts.AllowRefreshTokenFlow();

        opts.AddDevelopmentSigningCertificate()
            .AddDevelopmentEncryptionCertificate();

        opts.UseAspNetCore()
            .EnableTokenEndpointPassthrough();
    })
    .AddValidation(opts =>
    {
        opts.UseLocalServer();
        opts.UseAspNetCore();
    });

builder.Services.AddControllers();
```

OpenIddict's validation middleware uses `UseLocalServer()` to automatically trust tokens issued by the same application — no separate JWT Bearer configuration is needed.

---

## 3. Token endpoint controller

Add a controller to handle the `/connect/token` endpoint. Bedrock will call this with the `urn:bedrock:exchange` custom grant:

```csharp
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;

[ApiController]
public class TokenController : ControllerBase
{
    [HttpPost("~/connect/token")]
    public async Task<IActionResult> Exchange(
        [FromServices] IConfiguration config)
    {
        var request = HttpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("OpenIddict request not found.");

        if (request.GrantType == "urn:bedrock:exchange")
        {
            // Validate the internal assertion issued by Bedrock's token issuer
            var assertion = request.GetParameter("bedrock_assertion")?.Value?.ToString();
            if (string.IsNullOrEmpty(assertion))
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var identity = ValidateBedrockAssertion(
                assertion,
                config["Jwt:InternalSigningKey"]!,
                config["OpenIddict:Issuer"]!);

            if (identity is null)
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            // Set claim destinations — controls which claims appear in the access token
            foreach (var claim in principal.Claims)
            {
                claim.SetDestinations(OpenIddictConstants.Destinations.AccessToken);
            }

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        return BadRequest(new OpenIddictResponse
        {
            Error = OpenIddictConstants.Errors.UnsupportedGrantType
        });
    }

    private static ClaimsIdentity? ValidateBedrockAssertion(
        string assertion, string signingKey, string issuer)
    {
        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(signingKey));
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();

        try
        {
            var principal = handler.ValidateToken(assertion, new TokenValidationParameters
            {
                ValidIssuer              = issuer,
                ValidAudience            = "openiddict",
                IssuerSigningKey         = key,
                ValidateIssuerSigningKey = true,
                ValidateLifetime         = true,
                ClockSkew                = TimeSpan.Zero
            }, out _);

            return (ClaimsIdentity)principal.Identity!;
        }
        catch
        {
            return null;
        }
    }
}
```

The assertion is a short-lived JWT signed by Bedrock's internal key. The handler validates it, then re-signs the claims as an OpenIddict-issued access token.

---

## 4. IBedrockTokenIssuer implementation

```csharp
using Crestacle.Bedrock.Core.DTOs;
using Crestacle.Bedrock.Core.Interfaces.Services;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

public class OpenIddictBedrockTokenIssuer(
    IHttpClientFactory httpClientFactory,
    IConfiguration config) : IBedrockTokenIssuer
{
    public async Task<AccessTokenDescriptor> IssueAccessTokenAsync(
        Guid userId,
        string email,
        IEnumerable<string> roles,
        string? tenantId,
        IDictionary<string, string>? extraClaims,
        CancellationToken ct = default)
    {
        // Build a short-lived assertion that the token endpoint will validate
        var assertion = BuildAssertion(userId, email, roles, tenantId, extraClaims);

        // Call OpenIddict's token endpoint
        var client   = httpClientFactory.CreateClient("openiddict");
        var response = await client.PostAsync("connect/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"]        = "urn:bedrock:exchange",
                ["bedrock_assertion"] = assertion
            }), ct);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);

        var accessToken = doc.RootElement.GetProperty("access_token").GetString()!;

        // Parse the issued token to extract jti and expiry for Bedrock's session tracking
        var parsed  = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
        var jti     = parsed.Id;
        var expires = parsed.ValidTo;

        return new AccessTokenDescriptor(accessToken, jti, expires);
    }

    private string BuildAssertion(
        Guid userId,
        string email,
        IEnumerable<string> roles,
        string? tenantId,
        IDictionary<string, string>? extraClaims)
    {
        var signingKey = config["Jwt:InternalSigningKey"]!;
        var issuer     = config["OpenIddict:Issuer"]!;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,   userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        if (tenantId is not null)
            claims.Add(new Claim("tenant_id", tenantId));

        if (extraClaims is not null)
            foreach (var (key, value) in extraClaims)
                claims.Add(new Claim(key, value));

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer:   issuer,
            audience: "openiddict",
            claims:   claims,
            expires:  DateTime.UtcNow.AddMinutes(1),  // very short-lived assertion
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

Register the named `HttpClient` to point at your own application:

```csharp
builder.Services.AddHttpClient("openiddict", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["OpenIddict:Issuer"]!);
});
```

---

## 5. Application seeding

OpenIddict requires all OAuth clients to be registered in its application store. Seed the Bedrock server-to-server client at startup:

```csharp
public class OpenIddictSeeder(IServiceProvider services) : IHostedService
{
    public async Task StartAsync(CancellationToken ct)
    {
        using var scope = services.CreateScope();
        var manager = scope.ServiceProvider
            .GetRequiredService<IOpenIddictApplicationManager>();

        if (await manager.FindByClientIdAsync("bedrock", ct) is null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId   = "bedrock",
                // Bedrock's token issuer uses server-to-server exchange,
                // not a client secret — leave ClientSecret null.
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.Custom
                        + "urn:bedrock:exchange"
                }
            }, ct);
        }
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
```

```csharp
builder.Services.AddHostedService<OpenIddictSeeder>();
```

---

## 6. Migrations

Run migrations after updating the DbContext with `UseOpenIddict()`:

```bash
dotnet ef migrations add AddOpenIddict --context AppDbContext
dotnet ef database update --context AppDbContext
```

---

## 7. Protecting your own endpoints

Use OpenIddict's validation scheme instead of JWT Bearer:

```csharp
using OpenIddict.Validation.AspNetCore;

[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
[HttpGet("api/orders")]
public IActionResult GetOrders() { ... }
```

Or set it as the default scheme so `[Authorize]` picks it up automatically:

```csharp
builder.Services
    .AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
```

---

## 8. Downstream services (separate apps)

Services in other applications validate tokens using OpenIddict's OIDC discovery endpoint:

```csharp
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.Authority = "https://auth.yourapp.com";
        opts.Audience  = "yourapp";
        // OpenIddict exposes /.well-known/openid-configuration automatically
    });
```

---

## Configuration reference

Add to `appsettings.json`:

```json
{
  "Jwt": {
    "InternalSigningKey": "...",
    "ExternalTokenIssuer": true
  },
  "OpenIddict": {
    "Issuer": "https://yourapp.com"
  }
}
```

| Key | Purpose |
|---|---|
| `Jwt.InternalSigningKey` | Signs Bedrock's internal assertions and short-lived tokens (MFA, step-up, enrollment). Never exposed externally. |
| `Jwt.ExternalTokenIssuer` | Must be `true` so Bedrock skips installing its own JWT Bearer scheme. |
| `OpenIddict.Issuer` | The canonical URL of your OpenIddict server. Used as the `iss` claim and in OIDC discovery. |
