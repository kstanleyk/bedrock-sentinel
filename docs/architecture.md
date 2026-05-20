# Architecture

## Package layers

Bedrock is split across three layers. Each layer only depends on layers below it, so you can reference only what you need.

```
┌─────────────────────────────────────────────────────┐
│  Presentation                                        │
│  Crestacle.Bedrock.AspNetCore                        │
│  Crestacle.Sentinel.AspNetCore                       │
├─────────────────────────────────────────────────────┤
│  Infrastructure                                      │
│  Crestacle.Bedrock.EntityFramework                   │
│  Crestacle.Bedrock.Redis                             │
│  Crestacle.Bedrock.HaveIBeenPwned                    │
│  Crestacle.Sentinel.EntityFramework                  │
├─────────────────────────────────────────────────────┤
│  Domain                                              │
│  Crestacle.Bedrock.Core                              │
│  Crestacle.Sentinel.Core                             │
└─────────────────────────────────────────────────────┘
```

### Crestacle.Bedrock.Core

The domain layer. Contains entities, interfaces, DTOs, options, enumerations, and exceptions. Has no dependency on ASP.NET Core, EF Core, or any infrastructure package — only `Microsoft.Extensions.DependencyInjection.Abstractions` and `Microsoft.Extensions.Options`.

Key contents:
- **Entities** — `UserCredential`, `Session`, `RefreshToken`, `ApiKey`, `PasskeyCredential`, `ExternalIdentity`, `MfaChallenge`, `StepUpChallenge`, `RecoveryCode`, `AuditEntry`, `ConsentRecord`, `Invitation`, and more
- **Repository interfaces** — one interface per entity, e.g. `ICredentialRepository`, `ISessionRepository`
- **Service interfaces** — `ICredentialService`, `IMfaService`, `ITokenService`, `IBedrockTokenIssuer`, `IPasswordValidator`, and others
- **Extension point interfaces** — `IBedrockCache`, `IBedrockClaimsEnricher`, `IBedrockEventPublisher`, `IEmailSender`, `ISmsSender`, `IExternalIdentityValidator`, `ITenantContext`
- **Options** — `BedrockOptions` and its 12 nested option classes

### Crestacle.Bedrock.AspNetCore

The presentation layer. Contains all HTTP controllers, middleware, and the concrete implementations of core services (JWT, Argon2, TOTP, FIDO2).

Key contents:
- **Controllers** — 10 controllers covering auth, account, admin, API keys, sessions, MFA, passkeys, step-up, audit, and JWKS discovery
- **Middleware** — exception mapping, API key authentication, token scope enforcement
- **Services** — `JwtService`, `Argon2idPasswordHasher`, `TotpService`, `OtpService`, `PasskeyService`, `SessionService`
- **DI registration** — `AddBedrockAspNetCore()`, `AddBedrockControllers()`, and the `IBedrockBuilder` fluent interface

### Crestacle.Bedrock.EntityFramework

The data layer. Contains EF Core repositories, entity configurations, `BedrockContext`, and an in-memory cache implementation.

Key contents:
- **BedrockContext** — base `DbContext` with all 20 `DbSet<>` properties, `"auth"` schema, UTC converters, and optional tenant query filters
- **Repositories** — EF Core implementations of all 19 repository interfaces from Core
- **MemoryBedrockCache** — process-local `IBedrockCache` suitable for single-node deployments

### Crestacle.Bedrock.Redis

Provides `RedisBedrockCache`, a distributed `IBedrockCache` implementation backed by StackExchange.Redis. Drop-in replacement for the default in-memory cache in multi-pod deployments.

### Crestacle.Bedrock.HaveIBeenPwned

Provides an `IPasswordValidator` implementation that checks passwords against the Have I Been Pwned k-anonymity API. Registered with `.AddHaveIBeenPwnedPasswordValidator()` alongside the built-in validators.

### Crestacle.Bedrock.Sentinel

Bridge package connecting Bedrock and Sentinel. Provides unified DI registration, a `BedrockTenantContextAdapter` that forwards Bedrock's tenant context to Sentinel, and a `SentinelClaimsEnricher` that embeds RBAC permissions into Bedrock JWTs.

---

## Extension points

All external dependencies — email, SMS, event publishing, caching, claims enrichment, token issuance — are registered as null objects by default and replaced using the `IBedrockBuilder` fluent API:

| Interface | Default | Replace with |
|---|---|---|
| `IEmailSender` | `NullEmailSender` | `.WithEmailSender<T>()` |
| `ISmsSender` | `NullSmsSender` | `.WithSmsSender<T>()` |
| `IBedrockEventPublisher` | `NullBedrockEventPublisher` | `.WithEventPublisher<T>()` |
| `IBedrockClaimsEnricher` | `NullBedrockClaimsEnricher` | `.WithClaimsEnricher<T>()` |
| `IBedrockCache` | `NullBedrockCache` | `.WithCache<T>()` or `.WithRedisCache()` |
| `IPasswordHasher` | `Argon2idPasswordHasher` | `.WithPasswordHasher<T>()` |
| `IBedrockTokenIssuer` | `JwtBedrockTokenIssuer` | `.WithTokenIssuer<T>()` |
| `ITenantContext` | `NullTenantContext` | `.WithTenantContext<T>()` |
| `IExternalIdentityValidator` | _(none)_ | `.WithExternalIdentityValidator<T>()` (multiple allowed) |

Because registrations use `TryAdd*`, you can register your own implementation before calling `AddBedrockAspNetCore()` and it will take precedence.

---

## Key design decisions

**Schema isolation.** All Bedrock tables are created in the `"auth"` schema by default, keeping them separate from application tables even when sharing a single database.

**Interface-first.** Every service and repository is behind an interface defined in Core. This makes unit testing straightforward and allows any implementation to be swapped without changing consumer code.

**Null object defaults.** Features that require external infrastructure (email, SMS, Redis, event bus) are no-ops until you wire them up. The application starts and runs correctly without them — it just won't send emails or persist cache entries across nodes.

**Token scope enforcement.** The middleware pipeline includes a `BedrockScopeMiddleware` that prevents enrollment tokens from being used on regular endpoints and vice versa, enforcing the intended token lifecycle at the HTTP layer.

**Argon2id hashing.** Passwords are hashed with Argon2id by default. The `IPasswordHasher` interface can be replaced if a different algorithm is required.
