# Crestacle.Bedrock.HaveIBeenPwned

Breached-password detection for Crestacle Bedrock via the [Have I Been Pwned](https://haveibeenpwned.com/API/v3#SearchingPwnedPasswordsByRange) k-anonymity range API.

## What's in this package

- **`HibpPasswordValidator`** — `IPasswordValidator` implementation that queries HIBP using the k-anonymity SHA-1 prefix approach (your full password hash is **never** sent to the API)
- **`AddHaveIBeenPwnedPasswordValidator()`** — DI registration extension that registers the validator and its `HttpClient`

## Quick start

```csharp
builder.Services
    .AddBedrockAspNetCore(opts => { ... })
    .AddHaveIBeenPwnedPasswordValidator();
```

Once registered, the validator is automatically invoked during registration and password-change flows managed by `Crestacle.Bedrock.AspNetCore`.

## Documentation

- [HIBP integration guide](../../docs/integrations/hibp.md) — setup, combining with the built-in deny-list, and network failure behaviour
