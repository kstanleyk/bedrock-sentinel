# Have I Been Pwned integration

The `Crestacle.Bedrock.HaveIBeenPwned` package adds breach detection to password validation. When enabled, passwords are checked against the [Have I Been Pwned](https://haveibeenpwned.com/Passwords) k-anonymity API during registration and password changes. Passwords that appear in known data breaches are rejected.

The k-anonymity model means only the first 5 characters of the SHA-1 hash are sent to the API — the full password is never transmitted.

---

## Install

```bash
dotnet add package Crestacle.Bedrock.HaveIBeenPwned
```

---

## Registration

```csharp
builder.Services
    .AddBedrockEntityFramework<AppDbContext>()
    .AddBedrockAspNetCore(opts => { ... })
    .AddHaveIBeenPwnedPasswordValidator()
    .AddBedrockControllers();
```

`AddHaveIBeenPwnedPasswordValidator()` registers the HIBP validator alongside the built-in complexity validator. Both run on every password submission.

---

## Combining with the common password deny-list

For maximum coverage, enable both the HIBP validator and the built-in common password deny-list:

```csharp
builder.Services.AddBedrockAspNetCore(opts =>
{
    opts.Password.CommonPasswordDenyListPath = "embedded";
    // ...
})
.AddHaveIBeenPwnedPasswordValidator();
```

The built-in list blocks the top 1,000 most common passwords without a network call, while HIBP covers billions of breached passwords.

---

## Network dependency

HIBP validation requires an outbound HTTPS call to `api.pwnedpasswords.com` during password submission. If the API is unreachable, the validator fails closed — the password is rejected rather than accepted. This is the secure default.

If you need to allow passwords through when the API is unavailable, you can implement a custom `IPasswordValidator` that wraps the HIBP validator with a fallback.
