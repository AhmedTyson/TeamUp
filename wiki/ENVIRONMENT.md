# TeamUp — Environment Variables & Configuration Reference

> **Single Source of Truth** for all configuration settings consumed by the TeamUp ASP.NET Core 11 Web API and React 19 Vite SPA.

> **Sources:**
> - [backend/src/TeamUp.Api/appsettings.json](../backend/src/TeamUp.Api/appsettings.json)
> - [backend/src/TeamUp.Api/appsettings.Development.json](../backend/src/TeamUp.Api/appsettings.Development.json)
> - [backend/src/TeamUp.Api/appsettings.Production.example.json](../backend/src/TeamUp.Api/appsettings.Production.example.json)
> - [backend/src/TeamUp.Api/Extensions/SecurityPipelineExtensions.cs](../backend/src/TeamUp.Api/Extensions/SecurityPipelineExtensions.cs)
> - [backend/src/TeamUp.Application/Common/Security/SecurityOptions.cs](../backend/src/TeamUp.Application/Common/Security/SecurityOptions.cs)
> - [frontend/.env.example](../frontend/.env.example)

---

## Table of Contents

1. [Application & Runtime Host](#application--runtime-host)
2. [Database Connection & Provider Parameters](#database-connection--provider-parameters)
3. [Authentication, JWT & Security Controls](#authentication-jwt--security-controls)
4. [Email & Digest Configuration](#email--digest-configuration)
5. [CORS, Proxy Trust & Rate Limiting](#cors-proxy-trust--rate-limiting)
6. [Development Seeding & Fixtures](#development-seeding--fixtures)
7. [Frontend Environment Variables (`frontend/`)](#frontend-environment-variables-frontend)
8. [Logging & Secrets Safety Checklist](#logging--secrets-safety-checklist)

---

## Application & Runtime Host

| Key | Default (`appsettings.json` / Dev) | Production Target | Description |
|---|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Development` | `Production` | Controls OpenAPI mapping, HTTPS redirection, SQLite fallback, and strict startup validation. |
| `ASPNETCORE_URLS` | `http://localhost:5186` (via `launchSettings.json`) | `http://+:5186` | Host bindings and ports. |
| `AllowedHosts` | `*` | Specific domain(s) | Host headers accepted by Kestrel. |
| `Hosting:EnableHttpsRedirection` | `true` (outside Dev) | `true` (or `false` if edge terminates TLS) | Toggles `app.UseHttpsRedirection()` outside `Development`. |

---

## Database Connection & Provider Parameters

| Key | Default (Dev) | Description |
|---|---|---|
| `ConnectionStrings:TeamUpDb` | Configured via `user-secrets` | Primary SQL Server (or SQLite) connection string checked first by `Program.cs` and `DependencyInjection.cs`. |
| `ConnectionStrings:DefaultConnection` | `—` | Secondary fallback connection string name if `TeamUpDb` is unset. |
| `ConnectionStrings:SqliteFallback` | `Data Source=TeamUp.dev.db` | Local SQLite file path used when offline fallback triggers. |
| `DatabaseProvider` | `SqlServer` | Supports `"SqlServer"` (default) or `"Sqlite"`. |
| `UseSqliteFallbackIfOffline` | `true` (in `appsettings.Development.json`) / `false` | When `true` and SQL Server is unreachable via TCP probe, falls back to SQLite automatically. |
| `Database:AutoMigrate` | `true` in Dev when `Seed:Enabled=true`; otherwise `false` | Runs `EnsureCreatedAsync` (SQLite) or `MigrateAsync` (SQL Server) on startup. |

- **Production TLS Enforcement (`SecurityStartupValidator`):** Outside `Development`, `Production` SQL Server connections fail fast at startup if `TrustServerCertificate=True` or `Encrypt=False` is present (STRIDE THR-I-03).

---

## Authentication, JWT & Security Controls

| Key | Default | Production Rule | Purpose |
|---|---|---|---|
| `Jwt:Issuer` | `TeamUp` | `TeamUp` (or domain URI) | Issuer claim (`iss`) validated on JWTs. |
| `Jwt:Audience` | `TeamUp.Clients` | `TeamUp.Clients` | Audience claim (`aud`) validated on JWTs. |
| `Jwt:Key` | Dev key in `appsettings.Development.json` | Cryptographic 32+ byte random string | HMAC-SHA256 signing key (and Attendance HMAC key derivation). Startup fails fast outside Dev if missing, `< 32` UTF-8 bytes, or matching placeholder markers. |
| `Jwt:AccessTokenMinutes` | `15` | `15` | Lifespan of short-lived JWT access tokens (revoked via `SecurityStamp`). |
| `Jwt:RefreshTokenDays` | `7` | `7` – `30` | Lifespan of SHA-256 hashed refresh tokens. |
| `Jwt:PasswordResetTokenMinutes` | `15` | `15` (clamped `5–60`) | Expiration window for one-time password reset tokens. |
| `Admin:AllowRuntimePlatformAdminPromotion` | `false` | `false` | Blocks runtime `PATCH /api/v1/admin/students/{id}/role` elevation to `PlatformAdmin` (STRIDE THR-E-03). |
| `Attendance:MaxSelfCheckInsPerWindow` | `120` | `120` (tune per venue) | Caps per-session self check-ins within one 60-second rotating code window. |

---

## Email & Digest Configuration

| Key | Default (`appsettings.json` / Dev) | Production Rule | Description |
|---|---|---|---|
| `Email:Provider` | `Smtp` (`InMemory` in Dev) | **`Smtp` mandatory** | `InMemory` is rejected at startup in `Production` (STRIDE THR-S-03). |
| `Email:SmtpHost` | `""` | SMTP server host | Outbound SMTP hostname (e.g., `smtp.gmail.com`). |
| `Email:SmtpPort` | `587` | `587` / `465` | Outbound SMTP port. |
| `Email:UseSsl` | `true` | `true` | Enables TLS/STARTTLS for SMTP. |
| `Email:Username` / `Email:Password` | `null` | Vault-managed | Optional SMTP authentication credentials. |
| `Email:FromAddress` | `no-reply@teamup.edu` | Verified sender | Sender email address. |
| `Email:FromName` | `TeamUp Platform` | `TeamUp Platform` | Sender display name. |
| `Email:ResetPasswordUrlBase` | `https://teamup.edu/reset-password` | Frontend reset URL | Base URL for password reset links. |
| `Email:FrontendBaseUrl` | `https://teamup.edu` (Dev: `http://localhost:5173`) | Live SPA origin | Base URL used in digest and notification email links. |

---

## CORS, Proxy Trust & Rate Limiting

| Key | Default | Behavior |
|---|---|---|
| `Cors:AllowedOrigins` | `[]` (Dev defaults to `:5173` / `:4173`) | Explicit allowed origins for `FrontendPolicy` (`WithOrigins`, `AllowCredentials`). |
| `ForwardedHeaders:ForwardLimit` | `1` | Maximum proxy hop count. |
| `ForwardedHeaders:KnownProxies` / `KnownNetworks` | `[]` | Only honors `X-Forwarded-For` / `X-Forwarded-Proto` from explicitly trusted proxy IPs/CIDRs (STRIDE THR-S-02). |
| `RateLimiting:WindowSeconds` | `60` | Fixed window duration in seconds across all limiters. |
| `RateLimiting:AuthenticatedPermitLimit` | `120` | Global fixed-window limit per authenticated `StudentId` (`/health` exempt). |
| `RateLimiting:AnonymousPermitLimit` | `30` | Global fixed-window limit per sanitized `RemoteIpAddress`. |
| `RateLimiting:AuthPermitLimit` | `30` | Fixed-window limit on `/api/v1/auth/*` per `RemoteIpAddress`. |
| `RateLimiting:DirectoryPermitLimit` | `30` | Fixed-window limit on `GET /api/v1/students` per authenticated user (max 20 rows/page). |
| `RateLimiting:UploadPermitLimit` | `10` | Fixed-window limit on multipart upload endpoints (`POST /api/v1/images`, `POST /api/v1/students/me/profile/photo`). |

---

## Development Seeding & Fixtures

| Key | Default (`appsettings.json` / Dev) | Production Rule | Description |
|---|---|---|---|
| `Seed:Enabled` | `false` (`true` in `appsettings.Development.json`) | **`false` after bootstrap** | Runs `DbSeeder.SeedAsync` on startup (and triggers `AutoMigrate` in Dev). |
| `Seed:AdminPassword` | Dev fallback if omitted (`DevelopmentSeedPassword2026!`) | Unique 12+ char secret (never a documented sample) | Password hashed for `admin@teamup.local` (and Dev demo accounts). Rejected outside Dev if `< 12` chars or publicly documented. |

---

## Frontend Environment Variables (`frontend/`)

Defined in [`frontend/.env.example`](../frontend/.env.example):

| Variable | Default / Example | Description |
|---|---|---|
| `VITE_API_BASE_URL` | `""` (empty in Dev) | Production API origin (e.g., `https://your-site.runasp.net`, no trailing slash). Leave blank in Dev to use the Vite proxy. |
| `VITE_API_PROXY_TARGET` | `http://localhost:5186` | Target URL used by `vite.config.ts` dev/preview proxy for `/api`, `/images/people/student-*`, `/health`, and `/openapi`. |

---

## Logging & Secrets Safety Checklist

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

1. **Never commit passwords or secrets to Git:** `appsettings.json` in source control contains empty `ConnectionStrings` and empty `Jwt:Key`.
2. **Use .NET Secret Manager during local development:** Run `dotnet user-secrets set` inside `backend/src/TeamUp.Api`.
3. **Inject secrets via environment variables in production:** Use `__` double-underscore syntax (e.g., `ConnectionStrings__TeamUpDb`, `Jwt__Key`, `Email__Password`).
4. **Rotate JWT keys safely:** Rotating `Jwt:Key` invalidates all active JWTs and rotating attendance codes immediately.
