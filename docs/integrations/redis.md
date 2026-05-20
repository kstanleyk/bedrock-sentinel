# Redis integration

By default Bedrock uses an in-memory cache (`MemoryBedrockCache`) for JTI blacklisting, IP rate limiting, OTP send limits, TOTP replay prevention, and session creation locks. This cache is process-local and is not shared across pods.

In multi-pod deployments you must replace it with a distributed cache, otherwise:

- A revoked token may still be accepted by other pods
- IP rate limits may not be enforced across the full fleet
- OTP send limits may be bypassed
- Distributed session creation locks will not work

The `Crestacle.Bedrock.Redis` package provides a Redis-backed `IBedrockCache` as a drop-in replacement.

---

## Install

```bash
dotnet add package Crestacle.Bedrock.Redis
```

---

## Registration

Pass a connection string:

```csharp
builder.Services
    .AddBedrockEntityFramework<AppDbContext>()
    .AddBedrockAspNetCore(opts => { ... })
    .WithRedisCache(builder.Configuration.GetConnectionString("Redis"))
    .AddBedrockControllers();
```

Or pass an existing `IConnectionMultiplexer` if you manage the Redis connection yourself:

```csharp
var redis = await ConnectionMultiplexer.ConnectAsync("localhost:6379");

builder.Services
    .AddBedrockEntityFramework<AppDbContext>()
    .AddBedrockAspNetCore(opts => { ... })
    .WithRedisCache(redis)
    .AddBedrockControllers();
```

---

## Cache key prefixes

All keys written by Bedrock use the `Bedrock:` prefix to avoid collisions with other applications sharing the same Redis instance:

| Key pattern | Purpose |
|---|---|
| `Bedrock:revoked:{jti}` | JTI blacklist (revoked tokens) |
| `Bedrock:ip-fail:{ipBlock}` | Failed login counts per `/16` IP block |
| `Bedrock:otp-limit:{userId}:{purpose}` | OTP send counts per user per purpose |
| `Bedrock:totp-replay:{userId}:{code}` | TOTP replay prevention |
| `Bedrock:session-create-lock:{userId}` | Distributed lock during session creation |
| `Bedrock:claims:{userId}` | Cached claims enricher results (if enabled) |
| `Bedrock:roles:{userId}` | Cached role results (if Sentinel is used) |

---

## Single-node and development

The default in-memory cache is appropriate for:

- Single-node deployments
- Local development
- Integration tests

No configuration is required in these cases.
