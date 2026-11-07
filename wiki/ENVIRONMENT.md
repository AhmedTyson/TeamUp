# TeamUp — Environment Variables & Configuration Reference

> **Single Source of Truth** for all configuration settings consumed by the TeamUp ASP.NET Core Web API.

> **Sources:**
> - [src/TeamUp.Api/appsettings.json](../src/TeamUp.Api/appsettings.json)
> - [src/TeamUp.Api/appsettings.Development.json](../src/TeamUp.Api/appsettings.Development.json)
> - [src/TeamUp.Api/Program.cs](../src/TeamUp.Api/Program.cs)

---

## Table of Contents

1. [Application & Runtime Host](#application--runtime-host)
2. [Database Connection Parameters](#database-connection-parameters)
3. [Authentication & JWT Security](#authentication--jwt-security)
4. [Rate Limiting Configuration](#rate-limiting-configuration)
5. [Development Seeding & Fixtures](#development-seeding--fixtures)
6. [Logging & Diagnostics](#logging--diagnostics)
7. [Secrets Safety Checklist](#secrets-safety-checklist)

---

## Application & Runtime Host

| Key | Default (Dev) | Production Target | Description |
|---|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Development` | `Production` | Dictates pipeline behaviors (e.g., OpenAPI exposure, seeding). |
| `ASPNETCORE_URLS` | `http://localhost:5186` | `http://+:5186` | Host bindings and ports. |
| `AllowedHosts` | `*` | Specific Domain (e.g., `api.teamup.edu`) | Host headers accepted by Kestrel. |

---

## Database Connection Parameters

| Key | Default (Dev) | Description |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | `Server=localhost,14333;Database=TeamUpDB;User Id=sa;Password=...;TrustServerCertificate=True;` | Primary SQL Server connection string with connection retry and MARS enabled. |

### Connection String Options:
- `TrustServerCertificate=True`: Allowed in local development with self-signed container certificates; must be `False` with signed certs in production.
- `MultipleActiveResultSets=True`: Enables concurrent active query streams on a single database connection.

---

## Authentication & JWT Security

| Key | Default (Dev) | Production Rule | Purpose |
|---|---|---|---|
| `Jwt:Issuer` | `TeamUp` | `https://api.teamup.edu` | Issuer claim (`iss`) validated in access tokens. |
| `Jwt:Audience` | `TeamUp.Clients` | `https://teamup.edu` | Audience claim (`aud`) identifying valid clients. |
| `Jwt:Key` | `DevelopmentFallbackSecretKey...` | Cryptographic 64+ char random string | Secret key for signing HMAC-SHA256 JWT tokens. |
| `Jwt:AccessTokenMinutes` | `60` | `15` – `60` | Lifespan of short-lived JWT access tokens. |
| `Jwt:RefreshTokenDays` | `7` | `7` – `30` | Lifespan of cryptographically secure refresh tokens. |

---

## Rate Limiting Configuration

| Parameter | Value | Behavior |
|---|---|---|
| **Rate Limit Policy** | `auth-policy` | Fixed-window partition mapped per client IP. |
| **Permit Limit** | `60 requests` | Maximum allowed calls per time window. |
| **Window Duration** | `1 minute` | Fixed duration before quota resets. |
| **Rejection Code** | `429 Too Many Requests` | Returns RFC 7807 structured JSON envelope. |

---

## Development Seeding & Fixtures

| Key | Default (Dev) | Production Rule | Description |
|---|---|---|---|
| `Seed:Enabled` | `false` | **MUST BE `false`** | Triggers automatic migration application and demo data seeding on launch. |
| `Seed:AdminPassword` | — | Omit | Password used by `DbSeeder.cs` when creating the initial `PlatformAdmin` account. |

---

## Logging & Diagnostics

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Warning"
    }
  }
}
```

- In development, setting `Microsoft.EntityFrameworkCore.Database.Command` to `Information` outputs raw generated SQL queries with parameter bindings.
- In production, SQL command logging must be suppressed to avoid accidental leakage of personal identifiable information (PII).

---

## Secrets Safety Checklist

1. **Never commit passwords or secrets to Git:** `appsettings.json` in source control must contain empty connection strings and fallback dummy keys.
2. **Use .NET Secret Manager during local development:** Execute `dotnet user-secrets set` to store credentials outside the repository path.
3. **Inject secrets via environment variables in production:** Azure App Services, Docker Swarm, and Kubernetes inject parameters as environment variables (e.g., `ConnectionStrings__DefaultConnection`).
4. **Rotate JWT keys safely:** Generating a new `Jwt:Key` invalidates all existing sessions instantly, requiring users to log in again.
