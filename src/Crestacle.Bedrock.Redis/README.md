# Crestacle.Bedrock.Redis

Redis-backed `IBedrockCache` implementation for [Crestacle.Bedrock](https://github.com/crestacle/bedrock).

## Installation

```bash
dotnet add package Crestacle.Bedrock.Redis
```

## Usage

Call `WithRedisCache` on the builder returned by `AddBedrockAspNetCore`:

```csharp
// Option A — pass a connection string (connection opened lazily at first use)
builder.Services
    .AddBedrockEntityFramework<AppDbContext>()
    .AddBedrockAspNetCore(opts => { ... })
    .WithRedisCache("localhost:6379")
    .AddBedrockControllers();

// Option B — pass a pre-created IConnectionMultiplexer (share with other Redis users)
var redis = await ConnectionMultiplexer.ConnectAsync("localhost:6379");
builder.Services
    .AddBedrockAspNetCore(opts => { ... })
    .WithRedisCache(redis);
```

`WithRedisCache` replaces the default in-memory `IBedrockCache` with `RedisBedrockCache`,
distributing all Bedrock cache entries (JTI revocation, OTP rate limits, session locks, TOTP
replay prevention) across every pod connected to the same Redis instance.

## Cache keys

| Prefix | Description |
|---|---|
| `Bedrock:revoked:{jti}` | Revoked JWT JTI blacklist entries |
| `Bedrock:ip-fail:{ipBlock}` | Failed login attempt counters per IP block |
| `Bedrock:otp-limit:{userId}:{purpose}` | OTP send-rate-limit windows |
| `Bedrock:totp-replay:{userId}:{code}` | TOTP replay-prevention nonces |
| `Bedrock:session-create-lock:{userId}` | Distributed lock for atomic session creation |
| `Bedrock:claims:{userId}` | Cached enricher claims (when `WithClaimsEnricherCache` is used) |
| `Bedrock:roles:{userId}` | Cached enricher roles (when `WithClaimsEnricherCache` is used) |

## Documentation

- [Redis integration guide](../../docs/integrations/redis.md) — when to use Redis, setup options, and cache key reference
