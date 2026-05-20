# Crestacle.Bedrock.AspNetCore

ASP.NET Core integration layer for the Crestacle Bedrock authentication library.

## What's in this package

- **JWT service** — HS256/RS256 token generation and validation, challenge/step-up/enrollment tokens
- **Password hashing** — Argon2id via `Konscious.Security.Cryptography.Argon2`
- **MFA / TOTP** — time-based one-time password support via `OtpNet`
- **Passkeys / WebAuthn** — FIDO2 ceremony support via `Fido2NetLib`
- **API key authentication** — `X-Api-Key` header handler, key generation and revocation
- **Controllers** — `BedrockAuthController`, `BedrockAccountController`, `BedrockApiKeyController`, and more
- **Middleware** — `UseBedrock()` pipeline extension
- **Common password deny-list** — embedded list of weak/common passwords (compressed)
- **DI registration** — `AddBedrockAspNetCore()` and `AddBedrockControllers()` extension methods

## Quick start

```csharp
builder.Services
    .AddBedrockEntityFramework<YourDbContext>()
    .AddBedrockAspNetCore(opts =>
    {
        opts.Jwt.SigningKey = builder.Configuration["Jwt:SigningKey"];
        opts.Jwt.Issuer    = "https://yourapp.example.com";
        opts.Jwt.Audience  = "https://yourapp.example.com";
    })
    .AddBedrockControllers();

app.UseBedrock();
app.MapControllers();
```

## Documentation

- [Getting started](../../docs/getting-started.md) — complete walkthrough from install to first request
- [Deployment models](../../docs/deployment-models.md) — embedded, standalone, or external IDP
- [API reference](../../docs/api-reference.md) — all HTTP endpoints
- [Configuration reference](../../docs/configuration-reference.md) — all options with defaults
- [Security guide](../../docs/security.md) — production hardening checklist
