<div align="center">
  <br />
  <img src="https://api.iconify.design/lucide:users-round.svg?color=%233B82F6" width="140" alt="TeamUp Logo" />
  <br />

  # TeamUp — Student Collaboration & Gamification Platform
  **Clean Architecture .NET 10 Web API · Cross-Faculty Team Formation, Student Organizations & Gamification Engine**

  <br />

  [![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
  [![C# 13](https://img.shields.io/badge/C%23-13.0-239120?style=for-the-badge&logo=c-sharp&logoColor=white)](https://learn.microsoft.com/en-us/dotnet/csharp/)
  [![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-Web_API-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://asp.net/)
  [![EF Core 10](https://img.shields.io/badge/EF_Core-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://learn.microsoft.com/en-us/ef/core/)
  [![SQL Server 2022](https://img.shields.io/badge/Database-SQL_Server_2022-CC292B?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)](https://www.microsoft.com/en-us/sql-server/)
  [![JWT Auth](https://img.shields.io/badge/Auth-JWT_Bearer-000000?style=for-the-badge&logo=json-web-tokens&logoColor=white)](https://jwt.io/)
  [![Docker](https://img.shields.io/badge/Container-Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)](https://www.docker.com/)
  [![Tests 100/100](https://img.shields.io/badge/Tests-100%2F100_Passing-10b981?style=for-the-badge&logo=githubactions&logoColor=white)](https://github.com/AhmedTyson/TeamUp)
  ![Status](https://img.shields.io/badge/status-active_|_v1.0.0-10b981?style=flat-square)
  ![Architecture](https://img.shields.io/badge/architecture-Clean_Architecture_Grade_A-blue?style=flat-square)

</div>

<br />

---

<br />

Welcome to the **TeamUp** repository! **TeamUp** is an enterprise-grade academic collaboration and gamification platform engineered in **.NET 10 / C# 13**. It bridges higher-education academic programs and real-world project execution by connecting students across universities and faculties into cross-functional teams, facilitating student organization recruitment, scheduling campus workshops, verifying attendance via QR/PIN check-ins, and rewarding active student engagement through an immutable scoring ledger, progressive levels, and verifiable achievement badges.

---

## <img src="https://api.iconify.design/lucide:folder-tree.svg?color=%233B82F6" width="22" align="top" /> Repository Structure

```text
TeamUp/
├── src/
│   ├── TeamUp.Domain/             # Enterprise Entities, Invariants, Enums, Base Contracts (0 dependencies)
│   ├── TeamUp.Application/        # CQRS Services, DTOs, FluentValidators, Domain Event Handlers
│   ├── TeamUp.Infrastructure/     # EF Core 10 AppDbContext, Migrations, Interceptors, Seeders
│   └── TeamUp.Api/                # ASP.NET Core 10 Web API, Thin Controllers, Filters, Middleware
├── tests/
│   ├── TeamUp.UnitTests/          # Unit tests (Gamification formulas, FluentValidators, Interceptors)
│   └── TeamUp.IntegrationTests/   # End-to-end integration tests (WebApplicationFactory & DB context)
├── wiki/                          # 📚 Comprehensive project documentation home (guides, references & audits)
├── docs/                          # Architectural audits, API proposals, OpenAPI JSON & Postman files
├── ERD/                           # Peter Chen ERD (13 entities, 19 relationships), SVG/PNG diagrams
├── Mapping/                       # Relational schema mapping (23 tables across 5 tiers), 3NF checks
├── tasks/                         # 20-Phase implementation roadmap and modular task checklists
├── scripts/                       # PowerShell live HTTP API verification scripts (test_live_api.ps1)
└── TeamUp.sln                     # Visual Studio / Rider .NET 10 solution file
```

The repository follows a clean separation of concerns: `src/` contains the application projects, `tests/` houses the 100 automated test suites, `wiki/` is the documentation headquarters, and `ERD/` + `Mapping/` provide the relational database blueprints.

---

## <img src="https://api.iconify.design/lucide:sparkles.svg?color=%233B82F6" width="22" align="top" /> Key Features & Highlights

| Domain / Layer | Technology | Key Capabilities & Highlights |
|---|---|---|
| **Identity & Governance** | .NET 10, PBKDF2, JWT Bearer | Student registration with faculty affiliation, HMAC-SHA256 JWT, SHA-256 hashed refresh token rotation, 15-minute crypto password resets (redacted from HTTP payloads), public student directory, portfolio cards, and admin governance. |
| **Academic Catalog** | In-Memory Caching (`IMemoryCache`) | Multi-tenant university & faculty hierarchy, cached anonymous lookup (`/api/v1/universities/catalog`) for sub-5ms registration UX, administrative CRUD with cache invalidation. |
| **Projects & Sprint Tasks** | Clean DDD, CQRS | Cross-functional project creation, required skills matrix, vacancy recruitment, applicant vetting workflow, and Kanban task boards (`Todo` ➔ `InProgress` ➔ `Completed`). |
| **Organizations & Events** | Relational Schemas, Concurrency Tokens | Student clubs directory, executive committee assignments, multi-session event timetables, seat capacity limits with waitlists, and QR/PIN attendance check-ins. |
| **Gamification Engine** | Event Pub-Sub, Append-Only Ledger | Immutable `ScoreTransaction` financial ledger (`IAppendOnly`), automated level progression formula $\lfloor 1 + \sqrt{\text{pts}/100} \rfloor$, rule-driven badge evaluator, and in-app notifications inbox. |
| **Database Integrity** | SQL Server 2022, EF Core 10 | 23 tables, 29 foreign keys with **100% Zero Cascades** (`ON DELETE NO ACTION`), single-column soft-delete (`DeletedAt`), and 10 filtered unique indexes (`WHERE [DeletedAt] IS NULL`). |

---

## <img src="https://api.iconify.design/lucide:rocket.svg?color=%233B82F6" width="22" align="top" /> Quick Start Guide

### 1. Database Provisioning (Docker SQL Server 2022)

Launch Microsoft SQL Server 2022 mapped to port `14333`:

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong@Passw0rd!" \
   -p 14333:1433 --name teamup-sql --hostname teamup-sql \
   -d mcr.microsoft.com/mssql/server:2022-latest
```

### 2. Configure User Secrets

From the solution root, set local secrets using the .NET Secret Manager:

```bash
cd src/TeamUp.Api

# Set SQL Server connection string
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,14333;Database=TeamUpDB;User Id=sa;Password=YourStrong@Passw0rd!;TrustServerCertificate=True;MultipleActiveResultSets=True;"

# Set JWT secret key & parameters
dotnet user-secrets set "Jwt:Key" "SuperSecretDevelopmentKeyForTeamUpAppAtLeast32BytesLong2026!"
dotnet user-secrets set "Jwt:Issuer" "TeamUp"
dotnet user-secrets set "Jwt:Audience" "TeamUp.Clients"

# Enable development seeder with admin password
dotnet user-secrets set "Seed:Enabled" "true"
dotnet user-secrets set "Seed:AdminPassword" "Admin@TeamUp2026!"
```

### 3. Run Migrations & Start the Web API

```bash
# Apply EF Core migrations
dotnet ef database update --project ../TeamUp.Infrastructure --startup-project .

# Launch API host
dotnet run
```

- **Base HTTP URL:** `http://localhost:5186`
- **Base HTTPS URL:** `https://localhost:7173`
- **Health Check Endpoint:** `GET http://localhost:5186/health` (Returns `{"status": "Healthy"}`)
- **OpenAPI Document:** `http://localhost:5186/openapi/v1.json`

### 4. Default Seed Credentials

When `Seed:Enabled = true`, `DbSeeder.cs` provisions initial seed accounts:

| Role | Email | Password | Access Scope |
|---|---|---|---|
| **PlatformAdmin** | `admin@teamup.edu` | `Admin@TeamUp2026!` | Super-admin access across universities, accounts & recovery |
| **Student (Demo 1)** | `student1@cu.edu.eg` | `StudentPass123!` | Cairo University - Computer Science student profile |
| **Student (Demo 2)** | `student2@cu.edu.eg` | `StudentPass123!` | Cairo University - Information Systems student profile |

---

## <img src="https://api.iconify.design/lucide:flask-conical.svg?color=%233B82F6" width="22" align="top" /> Testing & Verification

Execute the complete automated test suite (57 Unit Tests + 43 Integration Tests):

```bash
# Run all automated tests
dotnet test

# Run unit tests only
dotnet test tests/TeamUp.UnitTests

# Run integration tests only (in-memory WebApplicationFactory)
dotnet test tests/TeamUp.IntegrationTests

# Run PowerShell live API runner against running host
pwsh scripts/test_live_api.ps1
```

```text
Passed!  - Failed: 0, Passed: 57, Skipped: 0, Total: 57 (TeamUp.UnitTests.dll)
Passed!  - Failed: 0, Passed: 43, Skipped: 0, Total: 43 (TeamUp.IntegrationTests.dll)
Total: 100 / 100 Passing Tests (100% Pass Rate)
```

---

## <img src="https://api.iconify.design/lucide:book-open.svg?color=%233B82F6" width="22" align="top" /> Documentation Home

All technical documentation, architectural decision records, and operational guides live in **[`wiki/`](wiki/Home.md)**:

| Section | Guides & Specifications |
|---|---|
| **Start Here** | [System Overview](wiki/System%20Overview.md) · [Getting Started Guide](wiki/Getting%20Started%20Guide.md) · [Development Guidelines](wiki/Development%20Guidelines.md) |
| **Architecture** | [Architecture Overview](wiki/Architecture.md) · [Technology Stack & Architecture](wiki/Technology%20Stack%20%26%20Architecture.md) · [Backend Services](wiki/Backend%20Services.md) · [Database Design & Mapping](wiki/Database%20Design%20%26%20Relational%20Mapping.md) |
| **Operations** | [Infrastructure](wiki/Infrastructure.md) · [Deployment Guide](wiki/DEPLOYMENT.md) · [Environment Configuration](wiki/ENVIRONMENT.md) · [Release Sign-off](wiki/RELEASE_SIGN_OFF.md) |
| **API Reference** | [API Reference](wiki/API%20Reference.md) · [API Endpoints Reference](wiki/API%20Endpoints%20Reference.md) · [Routes Appendix](wiki/ROUTES-APPENDIX.md) |
| **Audits & AI** | [Clean Code & Architecture Audit](wiki/Clean%20Code%20%26%20Architecture%20Audit.md) · [Adaptable AI Engine Research](wiki/Adaptable%20AI%20Engine%20Research.md) |

➡️ **Full navigation index:** [`wiki/Home.md`](wiki/Home.md)

---

## <img src="https://api.iconify.design/lucide:database.svg?color=%233B82F6" width="22" align="top" /> Database Design & Relational Mapping

| Visual Deliverable | Specification | Path |
|---|---|---|
| **Peter Chen ERD** | 13 Entities, 19 Relationships, Weak Entities, Identifying Relationships | [ERD/TeamUp-Chen-ERD.png](ERD/TeamUp-Chen-ERD.png) · [ERD/description.md](ERD/description.md) |
| **Relational Schema** | 23 Tables across 5 Tiers, Row-of-Cells Style, 29 Foreign Keys, 10 Filtered UQ Indexes | [Mapping/TeamUp-Relational-Mapping.png](Mapping/TeamUp-Relational-Mapping.png) · [Mapping/description.md](Mapping/description.md) |

```mermaid
graph LR
    subgraph Conceptual["Chen ERD Model"]
        ERD[13 Entities / 19 Relationships]
    end
    subgraph Relational["Relational Mapping (3NF)"]
        MAP[23 Tables across 5 Tiers]
    end
    subgraph Hardening["Production Schema Hardening"]
        D1[Zero Physical Cascades (NO ACTION)]
        D4[Single-Column Soft-Delete (DeletedAt)]
        D5[Filtered Unique Indexes (WHERE DeletedAt IS NULL)]
        D10[Append-Only Score Ledger (IAppendOnly)]
    end
    ERD --> MAP
    MAP --> D1 & D4 & D5 & D10
```

---

## <img src="https://api.iconify.design/lucide:users.svg?color=%233B82F6" width="22" align="top" /> Engineering Team

- **Ahmed Elsayed (@AhmedTyson)** — *Tech Lead & M1 Lead (Auth, Core Identity, Catalog, Platform Governance)* · [github.com/AhmedTyson](https://github.com/AhmedTyson)
- **Ossama** — *Module 2 Lead (Skills Master Catalog, Projects, Member Roles, Sprint Tasks)*
- **Ibrahim (@hemataher2026-dotcom)** — *Module 3 Lead (Student Organizations, Activities, Multi-Session Events, Attendance)*
- **Youssef (@yousef2792006-cpu)** — *Module 4 Lead (Gamification Ledger, Badges Engine, Notifications, OpenAPI)*

---

## <img src="https://api.iconify.design/lucide:scale.svg?color=%233B82F6" width="22" align="top" /> License

Licensed under the [MIT License](LICENSE) — TeamUp Academic Collaboration Platform Deliverable.
