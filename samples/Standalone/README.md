# Standalone sample

Demonstrates the **standalone deployment model**: a dedicated authentication microservice that wraps only Bedrock, with no business data of its own.

```
Auth Service (this app)           Your API (separate service)
├── api/bedrock/auth/*            ├── api/products/*
├── api/bedrock/account/*         └── Validates JWTs from auth service
├── api/bedrock/admin/*               via /.well-known/jwks.json
└── /.well-known/jwks.json
```

Other services in your system validate tokens issued by this service against its public JWKS endpoint — no callbacks to this service are needed on each request.

## Run

```bash
dotnet run
```

## Consuming the JWKS endpoint from another service

Point other ASP.NET Core services' JWT Bearer configuration at this service:

```csharp
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.Authority       = "https://localhost:5002";
        opts.Audience        = "yourapp";
        opts.MetadataAddress = "https://localhost:5002/.well-known/jwks.json";
    });
```

The JWKS endpoint is always at `/.well-known/jwks.json`, regardless of the Bedrock base path setting. It returns an empty `keys` array when HS256 (symmetric) signing is used — switch to RS256 (`Jwt.SigningCertificate`) to expose the public key.

## Key files

| File | Purpose |
|---|---|
| `AuthDbContext.cs` | Inherits `BedrockContext` with no extra entities |
| `Infrastructure/DevEmailSender.cs` | Logs emails to console |
| `Program.cs` | DI registration — Bedrock only, no business controllers |

## Relation to documentation

See [deployment models — standalone](../../docs/deployment-models.md#model-2-standalone-auth-service) for a full explanation of this pattern.
