# Embedded sample

Demonstrates the **embedded deployment model**: a single ASP.NET Core application where Bedrock auth tables and application business tables coexist in the same SQLite database.

```
App
├── api/bedrock/auth/*      ← Bedrock auth endpoints
├── api/bedrock/account/*   ← Bedrock account endpoints
└── api/orders/*            ← Application business API (JWT-protected)

Database (embedded.db)
├── auth.*                  ← Bedrock tables (users, sessions, tokens…)
└── public.*                ← Application tables (Orders)
```

## Run

```bash
dotnet run
```

The database is created automatically. Emails are logged to the console.

## Try it

**1. Register**
```http
POST http://localhost:5001/api/bedrock/auth/register
Content-Type: application/json

{ "email": "alice@example.com", "password": "CorrectHorseBatteryStaple1!" }
```

**2. Confirm email** — copy the token printed to the console:
```http
POST http://localhost:5001/api/bedrock/auth/confirm-email
Content-Type: application/json

{ "token": "<token-from-console>" }
```

**3. Log in**
```http
POST http://localhost:5001/api/bedrock/auth/login
Content-Type: application/json

{ "email": "alice@example.com", "password": "CorrectHorseBatteryStaple1!" }
```

**4. Call the business API** using the access token from step 3:
```http
GET http://localhost:5001/api/orders
Authorization: Bearer <accessToken>
```

## Key files

| File | Purpose |
|---|---|
| `AppDbContext.cs` | Inherits `BedrockContext`, adds `DbSet<Order>` |
| `Models/Order.cs` | Example business entity |
| `Controllers/OrdersController.cs` | JWT-protected business endpoint |
| `Infrastructure/DevEmailSender.cs` | Logs emails to console |
| `Program.cs` | DI registration |

## Relation to documentation

See [deployment models — embedded](../../docs/deployment-models.md#model-1-embedded) for a full explanation of this pattern.
