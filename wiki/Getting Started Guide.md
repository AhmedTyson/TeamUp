# TeamUp — Getting Started Guide

> **Local Development & Quickstart Manual** · Step-by-step instructions for provisioning Microsoft SQL Server, configuring user secrets, executing migrations, seeding demo fixtures, and running the automated test suite.

> **Sources:**
> - [src/TeamUp.Api/appsettings.json](../src/TeamUp.Api/appsettings.json)
> - [src/TeamUp.Api/Properties/launchSettings.json](../src/TeamUp.Api/Properties/launchSettings.json)
> - [src/TeamUp.Infrastructure/Seed/DbSeeder.cs](../src/TeamUp.Infrastructure/Seed/DbSeeder.cs)
> - [scripts/test_live_api.ps1](../scripts/test_live_api.ps1)

---

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Database Provisioning (Docker or Local SQL Server)](#database-provisioning-docker-or-local-sql-server)
3. [Configuration & User Secrets](#configuration--user-secrets)
4. [Applying Migrations & Database Seeding](#applying-migrations--database-seeding)
5. [Running the Application](#running-the-application)
6. [Interactive OpenAPI & Postman Testing](#interactive-openapi--postman-testing)
7. [Running the Automated Test Suite](#running-the-automated-test-suite)
8. [Troubleshooting Quick Hits](#troubleshooting-quick-hits)

---

## Prerequisites

Ensure the following runtimes and tools are installed:

- **.NET 10.0 SDK** (`dotnet --version` outputs `10.0.x`)
- **Microsoft SQL Server 2022** (either via Docker or native local instance)
- **Git** (for version control and patch management)
- **PowerShell 7+ (pwsh)** (for running live verification test scripts)
- **Docker Desktop** (optional, recommended for isolated SQL Server execution)

---

## Database Provisioning (Docker or Local SQL Server)

### Option A: Docker SQL Server 2022 (Recommended)

Run the official SQL Server 2022 Linux container exposed on port `14333`:

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong@Passw0rd!" \
   -p 14333:1433 --name teamup-sql --hostname teamup-sql \
   -d mcr.microsoft.com/mssql/server:2022-latest
```

### Option B: Local SQL Server / LocalDB

Alternatively, connect to a local SQL Server instance:
`Server=localhost;Database=TeamUpDB;Trusted_Connection=True;TrustServerCertificate=True;`

---

## Configuration & User Secrets

Sensitive settings (database connection string, JWT signing keys, admin seed password) must never be committed to source control. Configure them using .NET Secret Manager:

```bash
cd src/TeamUp.Api

# 1. Initialize User Secrets
dotnet user-secrets init

# 2. Set SQL Server Connection String
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,14333;Database=TeamUpDB;User Id=sa;Password=YourStrong@Passw0rd!;TrustServerCertificate=True;MultipleActiveResultSets=True;"

# 3. Configure JWT Secret Key (min 32 characters)
dotnet user-secrets set "Jwt:Key" "SuperSecretProductionGradeEncryptionKeyForTeamUpApplication2026!"
dotnet user-secrets set "Jwt:Issuer" "TeamUp"
dotnet user-secrets set "Jwt:Audience" "TeamUp.Clients"

# 4. Configure Development Seed Credentials
dotnet user-secrets set "Seed:Enabled" "true"
dotnet user-secrets set "Seed:AdminPassword" "Admin@TeamUp2026!"
```

---

## Applying Migrations & Database Seeding

EF Core migrations apply automatically when `Seed:Enabled = true` in development, or can be applied explicitly via the .NET CLI:

```bash
# Apply migrations using dotnet ef CLI
dotnet ef database update --project src/TeamUp.Infrastructure --startup-project src/TeamUp.Api
```

### Default Seed Credentials

When `Seed:Enabled` is activated, `DbSeeder.cs` idempotently provisions initial academic faculties and an administrative user:

| Account Role | Email | Default Password | Faculty Affiliation |
|---|---|---|---|
| **PlatformAdmin** | `admin@teamup.edu` | `Admin@TeamUp2026!` | Faculty of Computers & AI |
| **Student (M1 Seed)** | `student1@cu.edu.eg` | `StudentPass123!` | Cairo University - Computer Science |
| **Student (M2 Seed)** | `student2@cu.edu.eg` | `StudentPass123!` | Cairo University - Information Systems |

---

## Running the Application

Launch the ASP.NET Core Web API:

```bash
dotnet run --project src/TeamUp.Api
```

- **Base HTTP URL:** `http://localhost:5186`
- **Base HTTPS URL:** `https://localhost:7173`
- **Health Check Endpoint:** `GET http://localhost:5186/health` (Returns `{"status": "Healthy"}`)
- **Base API Route:** `http://localhost:5186/api/v1`

---

## Interactive OpenAPI & Postman Testing

### 1. OpenAPI v3 Specification
TeamUp exposes native .NET 10 OpenAPI document generation:
- **OpenAPI JSON Document:** `http://localhost:5186/openapi/v1.json`
- **Pre-generated Artifact:** [`docs/api/openapi.json`](../docs/api/openapi.json)

### 2. Postman Collection
Pre-configured collections with dynamic authentication token persistence live in `docs/api/postman/`:
- **Collection:** [`docs/api/postman/TeamUp_Collection.json`](../docs/api/postman/TeamUp_Collection.json)
- **Environment:** [`docs/api/postman/TeamUp_Environment.json`](../docs/api/postman/TeamUp_Environment.json)

*To test via Postman:*
1. Import both JSON files into Postman.
2. Select the `TeamUp Local` environment.
3. Execute `Auth > Login (Student)`. Postman's test script will automatically capture the returned JWT Bearer token into `{{accessToken}}` for subsequent requests.

### 3. Live PowerShell Runner
To execute automated end-to-end integration requests against a running server:

```powershell
pwsh scripts/test_live_api.ps1
```

---

## Running the Automated Test Suite

TeamUp features 100 automated tests covering unit domain math, validation rules, soft-delete interceptors, and full HTTP integration workflows:

```bash
# Run all unit and integration tests
dotnet test

# Run only unit tests
dotnet test tests/TeamUp.UnitTests

# Run only integration tests (spawns in-memory WebApplicationFactory)
dotnet test tests/TeamUp.IntegrationTests

# Run tests with detailed test names
dotnet test --logger "console;verbosity=detailed"
```

---

## Troubleshooting Quick Hits

| Symptom | Probable Cause | Corrective Action |
|---|---|---|
| **SQL Connection Refused (port 14333)** | SQL Server Docker container is stopped. | Run `docker ps` to verify; start container via `docker start teamup-sql`. |
| **401 Unauthorized on all endpoints** | Expired access token or missing `Authorization: Bearer <token>` header. | Call `POST /api/v1/auth/refresh` or re-login at `/api/v1/auth/login`. |
| **429 Too Many Requests on auth** | Partitioned rate limiter exceeded (60 requests/minute per IP). | Wait 60 seconds for the fixed window rate limiter to reset. |
| **409 Conflict on registration** | Email address already exists for an active user. | Use a distinct email address, or soft-delete the previous user. |
| **`Seed:AdminPassword` missing exception** | `Seed:Enabled=true` but no password configured in user secrets. | Run `dotnet user-secrets set "Seed:AdminPassword" "YourPassword!"`. |
