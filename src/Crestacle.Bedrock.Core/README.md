# Crestacle.Bedrock.Core

Core domain layer for the Crestacle Bedrock authentication library.

## What's in this package

- **Entities** — `UserCredential`, `Session`, `RefreshToken`, `ApiKey`, `PasskeyCredential`, `ExternalIdentity`, and more
- **Interfaces** — repository and service contracts consumed by the higher-level packages
- **Options** — `BedrockOptions` and nested option classes for JWT, password policy, lockout, MFA, etc.
- **Enumerations** — `AuditEventType`, `MfaMethod`, `MfaChallengeStatus`, and others
- **DTOs** — lightweight data transfer objects used across package boundaries
- **Exceptions** — typed domain exceptions (e.g., `BedrockException`)

This package has **no runtime dependencies** beyond `Microsoft.Extensions.DependencyInjection.Abstractions`
and is suitable for use in domain / application layers without pulling in ASP.NET Core.

## Related packages

| Package | Purpose |
|---|---|
| `Crestacle.Bedrock.AspNetCore` | JWT auth, controllers, middleware, DI registration |
| `Crestacle.Bedrock.EntityFramework` | EF Core repositories, caching, health checks |
| `Crestacle.Bedrock.HaveIBeenPwned` | Breached-password detection via HIBP |

## Documentation

- [Architecture](../../docs/architecture.md) — package layers, extension points, design decisions
- [Configuration reference](../../docs/configuration-reference.md) — all options with defaults
- [Getting started](../../docs/getting-started.md) — complete integration walkthrough
