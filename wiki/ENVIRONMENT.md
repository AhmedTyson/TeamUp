# TeamUp — Environment Variables & Configuration Reference

> **Single Source of Truth** for all configuration settings consumed by the TeamUp ASP.NET Core Web API.

> **Sources:**
> - [backend/src/TeamUp.Api/appsettings.json](../backend/src/TeamUp.Api/appsettings.json)
> - [backend/src/TeamUp.Api/appsettings.Development.json](../backend/src/TeamUp.Api/appsettings.Development.json)
> - [backend/src/TeamUp.Api/Program.cs](../backend/src/TeamUp.Api/Program.cs)

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
| `Jwt:Key` | Set in `appsettings.Development.json` | Cryptographic 64+ char random string | Secret key for signing HMAC-SHA256 JWT tokens. Startup fails fast outside Development if missing, `< 32` bytes, or a fallback default. |
| `Jwt:AccessTokenMinutes` | `15` | `15` | Lifespan of short-lived JWT access tokens (revoked immediately via `SecurityStamp`). |
| `Jwt:RefreshTokenDays` | `7` | `7` – `30` | Lifespan of cryptographically secure refresh tokens (stored SHA-256 hashed). |
| `Jwt:PasswordResetTokenMinutes` | `15` | `15` | Expiration window for one-time password reset tokens. |
| `Email:Provider` | `InMemory` (Dev only) | `Smtp` (Mandatory in Production) | Dispatches transactional emails via SMTP (`Templates/Emails/PasswordReset.html`). Startup fails in Production if set to `InMemory`. |
| `Admin:AllowRuntimePlatformAdminPromotion` | `false` | `false` | Blocks runtime `PATCH /api/v1/admin/students/{id}/role` elevation to `PlatformAdmin` (STRIDE THR-E-03). |
| `ForwardedHeaders:KnownProxies` / `KnownNetworks` | `[]` | Trusted proxy IPs / CIDRs | Only honors `X-Forwarded-For` / `X-Forwarded-Proto` when trusted proxies are explicitly configured (STRIDE THR-S-02). |

---

## Rate Limiting Configuration

| Parameter | Default Value | Behavior |
|---|---|---|
| **Global Authenticated (`RateLimiting:AuthenticatedPermitLimit`)** | `120 req / min` | Fixed-window per authenticated `StudentId` (`/health` exempt). |
| **Global Anonymous (`RateLimiting:AnonymousPermitLimit`)** | `30 req / min` | Fixed-window per sanitized `RemoteIpAddress` (never raw `X-Forwarded-For`). |
| **Auth Policy (`RateLimiting:AuthPermitLimit`)** | `30 req / min` | Fixed-window on `/api/v1/auth/*` per `RemoteIpAddress`. |
| **Directory Policy (`RateLimiting:DirectoryPermitLimit`)** | `30 req / min` | Fixed-window on `GET /api/v1/students` per authenticated user (max 20 rows/page). |
| **Rejection Code** | `429 Too Many Requests` | Returns `BaseResponse` JSON envelope with `Retry-After` header. |

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
