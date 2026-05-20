# External IDP sample

Demonstrates the **external IDP deployment model**: Bedrock manages credentials, MFA, sessions, and audit while a separate identity provider owns access token issuance.

A `MockIdpController` stands in for a real IDP (Auth0, Keycloak, Azure AD B2C, etc.), making this sample entirely self-contained.

```
┌──────────────────────────────┐
│  This application            │
│                              │
│  Bedrock handles:            │
│  - Credential storage        │
│  - MFA / passkeys            │
│  - Session tracking          │   ← ExternalTokenIssuer = true
│  - Audit                     │
│                              │
│  MockTokenIssuer delegates   │
│  token signing to:           │
│                              │
│  ┌────────────────────────┐  │
│  │  MockIdpController     │  │
│  │  POST /mock-idp/token  │  │
│  │  (dev/test only)       │  │
│  └────────────────────────┘  │
└──────────────────────────────┘
```

## Run

```bash
dotnet run
```

## How it works

1. `opts.Jwt.ExternalTokenIssuer = true` — Bedrock skips installing its own JWT Bearer scheme.
2. `MockTokenIssuer` (implements `IBedrockTokenIssuer`) signs access tokens with `ExternalIdp:SigningKey`.
3. JWT Bearer validation is configured manually using the same key, so incoming requests are authenticated correctly.
4. `opts.Jwt.InternalSigningKey` is still set — Bedrock uses it for short-lived internal tokens (MFA challenges, step-up tokens, enrollment tokens) that never leave the service.

## Try it

**1. Register and confirm** (same as other samples — emails appear in console)
```http
POST http://localhost:5003/api/bedrock/auth/register
Content-Type: application/json

{ "email": "alice@example.com", "password": "CorrectHorseBatteryStaple1!" }
```

**2. Log in** — the access token in the response is issued by `MockTokenIssuer` and signed with `ExternalIdp:SigningKey`:
```http
POST http://localhost:5003/api/bedrock/auth/login
Content-Type: application/json

{ "email": "alice@example.com", "password": "CorrectHorseBatteryStaple1!" }
```

**3. Call the mock IDP directly** (demonstrates what a real IDP token endpoint looks like):
```http
POST http://localhost:5003/mock-idp/token
Content-Type: application/json

{ "subject": "any-user-id", "email": "alice@example.com" }
```

## Replacing the mock with a real IDP

1. Remove `MockIdpController` and `MockTokenIssuer`.
2. Implement `IBedrockTokenIssuer` to call your IDP's token endpoint (e.g. client credentials + token exchange per RFC 8693).
3. Replace the manual `AddJwtBearer` configuration with your IDP's authority or JWKS URL:

```csharp
.AddJwtBearer(opts =>
{
    opts.Authority = "https://your-idp.com";
    opts.Audience  = "yourapp";
});
```

## Key files

| File | Purpose |
|---|---|
| `Auth/MockTokenIssuer.cs` | `IBedrockTokenIssuer` implementation — delegates signing to the mock IDP |
| `Auth/MockIdpController.cs` | Simulated IDP token endpoint (dev/test only) |
| `AppDbContext.cs` | Inherits `BedrockContext` with no extra entities |
| `Infrastructure/DevEmailSender.cs` | Logs emails to console |
| `Program.cs` | DI registration showing the external IDP wiring |

## Relation to documentation

See [deployment models — external IDP](../../docs/deployment-models.md#model-3-external-idp-consumer) and the [external IDP integration guide](../../docs/integrations/external-idp.md) for full details.
