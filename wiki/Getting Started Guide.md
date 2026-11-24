# TeamUp — Getting Started Guide

> **Local Development & Quickstart Manual** · Step-by-step instructions for provisioning SQL Server (or using the automatic Development SQLite fallback), configuring user secrets, executing migrations, seeding demo fixtures, starting the .NET 10 API & React 19 SPA, and running all test suites.

> **Sources:**
> - [backend/src/TeamUp.Api/appsettings.json](../backend/src/TeamUp.Api/appsettings.json)
> - [backend/src/TeamUp.Api/appsettings.Development.json](../backend/src/TeamUp.Api/appsettings.Development.json)
> - [backend/src/TeamUp.Api/Properties/launchSettings.json](../backend/src/TeamUp.Api/Properties/launchSettings.json)
> - [backend/src/TeamUp.Infrastructure/Seed/DbSeeder.cs](../backend/src/TeamUp.Infrastructure/Seed/DbSeeder.cs)
> - [frontend/package.json](../frontend/package.json)
> - [scripts/test_live_api.ps1](../scripts/test_live_api.ps1)

---

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Database Provisioning (Docker SQL Server, Local SQL Server, or Dev SQLite Fallback)](#database-provisioning-docker-sql-server-local-sql-server-or-dev-sqlite-fallback)
3. [Tooling, Configuration & User Secrets](#tooling-configuration--user-secrets)
4. [Applying Migrations & Database Seeding](#applying-migrations--database-seeding)
5. [Running the Backend API & React Frontend](#running-the-backend-api--react-frontend)
6. [Interactive OpenAPI & Postman Testing](#interactive-openapi--postman-testing)
7. [Running the Automated Test Suite](#running-the-automated-test-suite)
8. [Troubleshooting Quick Hits](#troubleshooting-quick-hits)

---

## Prerequisites

Ensure the following runtimes and tools are installed:

- **.NET 10.0 SDK** (`dotnet --version` outputs `10.0.x`)
- **Node.js 22+** & **npm** (for the React 19 + Vite frontend workspace)
- **Microsoft SQL Server 2022** (via Docker or native local instance — *optional in Development thanks to automatic SQLite fallback*)
- **Git** (for version control and patch management)
- **PowerShell 7+ (`pwsh`)** (for running live API verification and publish scripts)
- **Docker Desktop** (optional, recommended for isolated SQL Server 2022 execution)

---

## Database Provisioning (Docker SQL Server, Local SQL Server, or Dev SQLite Fallback)

### Option A: Docker SQL Server 2022 (Recommended)

Run the official SQL Server 2022 Linux container exposed on port `14333`:

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong@Passw0rd!" \
   -p 14333:1433 --name teamup-sql --hostname teamup-sql \
   -d mcr.microsoft.com/mssql/server:2022-latest
```

### Option B: Local SQL Server / LocalDB

Alternatively, connect to a local SQL Server instance:
`Server=localhost;Database=TeamUpDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True;`

### Option C: Zero-Setup Development SQLite Fallback

[`appsettings.Development.json`](../backend/src/TeamUp.Api/appsettings.Development.json) sets `"UseSqliteFallbackIfOffline": true`. If SQL Server is unreachable (checked via TCP probe in `DependencyInjection.cs`) or no SQL Server connection string is configured in `Development`, the API automatically falls back to a local SQLite database (`TeamUp.dev.db`), creates the schema, and applies any incremental local dev columns on startup.

---

## Tooling, Configuration & User Secrets

First, restore the local `dotnet-ef` (`10.0.14`) tool manifest from the repository root:

```bash
dotnet tool restore
```

Sensitive settings (database connection string, JWT signing keys, admin seed password) must never be committed to source control. Configure them using .NET Secret Manager:

```bash
cd backend/src/TeamUp.Api

# 1. Initialize User Secrets
dotnet user-secrets init

# 2. Set SQL Server Connection String
dotnet user-secrets set "ConnectionStrings:TeamUpDb" "Server=localhost,14333;Database=TeamUpDB;User Id=sa;Password=YourStrong@Passw0rd!;TrustServerCertificate=True;MultipleActiveResultSets=True;"

# 3. Configure JWT Secret Key (min 32 UTF-8 bytes)
dotnet user-secrets set "Jwt:Key" "<generate-a-random-64-byte-secret>"
dotnet user-secrets set "Jwt:Issuer" "TeamUp"
dotnet user-secrets set "Jwt:Audience" "TeamUp.Clients"

# 4. Configure Development Seed Credentials (min 12 chars)
dotnet user-secrets set "Seed:Enabled" "true"
dotnet user-secrets set "Seed:AdminPassword" "<choose-a-unique-12+-char-password>"
```

> **Note for Local Dev:** `appsettings.Development.json` already provides a development-only `Jwt:Key`, `"Email:Provider": "InMemory"`, and `"Seed:Enabled": true`. If `Seed:AdminPassword` is omitted in `Development`, `Program.cs` uses a local dev-only fallback password (`DevelopmentSeedPassword2026!`). Outside `Development`, `SecurityStartupValidator` strictly requires a secure `Jwt:Key`, `"Email:Provider": "Smtp"`, verified SQL TLS, and (if seeding is enabled) a unique `Seed:AdminPassword` of 12+ characters.

---

## Applying Migrations & Database Seeding

When using SQL Server, EF Core migrations apply automatically on startup when `Database:AutoMigrate = true` (default in `Development` when `Seed:Enabled = true`), or can be applied explicitly via the .NET CLI:

```bash
# Apply migrations using dotnet ef CLI (from repo root)
dotnet ef database update --project backend/src/TeamUp.Infrastructure --startup-project backend/src/TeamUp.Api
```

### Seeded Reference Data & Accounts

When `Seed:Enabled = true`, [`DbSeeder.cs`](../backend/src/TeamUp.Infrastructure/Seed/DbSeeder.cs) idempotently provisions **12 Egyptian universities**, **25 faculties**, **30 master skills**, **5 achievement badges**, and the following accounts:

| Account Role | Email | Password | Scope / Notes |
|---|---|---|---|
| **PlatformAdmin** | `admin@teamup.local` | *(your `Seed:AdminPassword`)* | Created in all environments when `Seed:Enabled=true`. |
| **OrgAdmin (Demo)** | `nour@cu.edu.eg` | *(your `Seed:AdminPassword`)* | **Development only** (`isDevelopment == true`). |
| **Showcase Students (6)** | `salma@cu.edu.eg`, `omar@capu.edu.eg`, `youssef@asu.edu.eg`, `hana@alexu.edu.eg`, `karim@guc.edu.eg`, `mariam@mans.edu.eg` | *(your `Seed:AdminPassword` in Dev; unusable random hash outside Dev)* | Pre-populated with profiles, portraits (`/images/people/*.jpg`), skills, points, and threshold badges. |

---

## Running the Backend API & React Frontend

### 1. Launch the ASP.NET Core Web API

```bash
dotnet run --project backend/src/TeamUp.Api
```

- **Base HTTP URL:** `http://localhost:5186`
- **Base HTTPS URL:** `https://localhost:7173`
- **Health Check Endpoint:** `GET http://localhost:5186/health` (Returns `{"status": "Healthy", "database": "Connected"}`)
- **Base API Route:** `http://localhost:5186/api/v1`

### 2. Launch the React 19 Frontend SPA

```bash
cd frontend
npm install
npm run dev
```

- **Frontend Dev Server:** `http://localhost:5173`
- **Preview Build Server:** `http://localhost:4173` (`npm run preview`)
- **Vite Proxy:** Automatically forwards `/api`, `/health`, `/openapi`, and `/images/people/student-*` to `VITE_API_PROXY_TARGET` (default `http://localhost:5186`, see [`frontend/.env.example`](../frontend/.env.example)).

---

## Interactive OpenAPI & Postman Testing

### 1. OpenAPI v3 Specification
TeamUp exposes native .NET 10 OpenAPI document generation in `Development`:
- **OpenAPI JSON Document:** `http://localhost:5186/openapi/v1.json`
- **Pre-generated Artifact:** [`docs/api/openapi.json`](../docs/api/openapi.json) (refreshed via `TEAMUP_EXPORT_API_DOCS=1 dotnet test backend/tests/TeamUp.IntegrationTests --filter "FullyQualifiedName~OpenApi_GeneratesValidV3Spec_AndExportsStaticFile"`)

### 2. Postman Collection
Pre-configured collections live in `docs/api/postman/`:
- **Collection:** [`docs/api/postman/TeamUp_Collection.json`](../docs/api/postman/TeamUp_Collection.json)
- **Environment:** [`docs/api/postman/TeamUp_Environment.json`](../docs/api/postman/TeamUp_Environment.json) (`baseUrl` set to `http://localhost:5186`)

*To test via Postman:*
1. Import both JSON files into Postman.
2. Select the `TeamUp Local Environment`.
3. Execute `01. Auth & Identity > Login` and store the returned `data.accessToken` in `{{accessToken}}` for authenticated requests.

### 3. Live PowerShell Runner
To execute automated end-to-end HTTP checks against a running server:

```powershell
pwsh scripts/test_live_api.ps1
```

---

## Running the Automated Test Suite

TeamUp features **263 automated tests** (117 Backend Unit + 75 Backend Integration + 71 Frontend Vitest) plus a Playwright E2E smoke suite:

```bash
# Run all backend unit and integration tests (192 tests)
dotnet test backend/TeamUp.sln

# Run only unit tests (117 tests)
dotnet test backend/tests/TeamUp.UnitTests

# Run only integration tests (75 tests using in-memory SQLite WebApplicationFactory)
dotnet test backend/tests/TeamUp.IntegrationTests

# Run frontend unit/component tests & production build check (71 tests)
cd frontend && npm run test && npm run build

# Run Playwright Chromium E2E smoke suite
cd frontend && npm run test:e2e
```

---

## Troubleshooting Quick Hits

| Symptom | Probable Cause | Corrective Action |
|---|---|---|
| **API boots on SQLite instead of SQL Server** | SQL Server container on port `14333` is stopped and `UseSqliteFallbackIfOffline=true` is set in `Development`. | Run `docker ps` to verify; start container via `docker start teamup-sql` and restart the API. |
| **401 Unauthorized on all endpoints** | Expired access token (`15 min`), missing `Authorization: Bearer <token>` header, or rotated `SecurityStamp`. | Call `POST /api/v1/auth/refresh` or re-login at `POST /api/v1/auth/login`. |
| **429 Too Many Requests** | Tiered fixed-window rate limiter exceeded (`30 req/min` on `/api/v1/auth/*`, anonymous, and directory; `120 req/min` authenticated). | Wait up to `60` seconds (per `Retry-After` header) for the window to reset. |
| **409 Conflict on registration** | Email address already exists for an active student (`DeletedAt IS NULL`). | Use a distinct email address, or soft-delete the previous user. |
| **Startup fails outside Development** | Missing/short `Jwt:Key`, `Email:Provider=InMemory`, `TrustServerCertificate=True`, or weak `Seed:AdminPassword`. | Satisfy `SecurityStartupValidator` production rules (see [ENVIRONMENT.md](./ENVIRONMENT.md)). |
