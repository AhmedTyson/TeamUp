# TeamUp — Contribution & Team Workflow Guidelines

## 1. Team Ownership Split (4 Members)

| Member | Focus Area | Entities / Tables Owned |
|---|---|---|
| **M1 (Tyson)** (Tech Lead) | Architecture, Auth, Identity & Governance | `University`, `Faculty`, `Student`, `StudentInterest`, `StudentProfile`, `AuditLog`, Base Entities, CI |
| **M2 (Ossama)** | Skills, Projects & Teams | `Skill`, `StudentSkill`, `Project`, `ProjectRequiredSkill`, `ProjectMember`, `ProjectApplication`, `ProjectTask` |
| **M3 (Ibrahim)** | Student Activities & Events | `Organization`, `OrganizationMember`, `MembershipApplication`, `Event`, `EventRegistration`, `EventSession`, `SessionAttendance` |
| **M4 (Youssef)** | Gamification, Alerts & Docs | `ScoreTransaction`, `Badge`, `StudentBadge`, `Notification`, OpenAPI, Postman, API Docs |

---

## 2. Git & Branching Strategy

- **Target branch:** All work merges into `main` via Pull Requests.
- **Branch naming convention:**
  - Feature: `feat/m<1|2|3|4>-<short-description>` (e.g., `feat/m2-project-crud`)
  - Fix: `fix/m<1|2|3|4>-<short-description>`
  - Docs: `docs/<short-description>`
- **Pull Request Rules:**
  - One task per PR.
  - At least 1 review required from another team member before merging.
  - CI must pass (build clean, zero warnings, all backend/frontend/E2E tests pass, zero SQL cascades).
  - Use squash-and-merge with a clean conventional commit message (`feat: ...`, `fix: ...`).

---

## 3. Database & EF Core Rules

1. **Single-Column Soft Delete Standard:**
   - Every entity except append-only ledgers (`ScoreTransaction`, `AuditLog`) implements `ISoftDelete` (`DeletedAt DATETIME2 NULL`).
   - Active records: `DeletedAt IS NULL`. Soft-deleted records: `DeletedAt IS NOT NULL`.
   - Physical deletion (`DELETE FROM`) is prohibited in application code. Use `db.Remove(...)` which our `SoftDeleteInterceptor` converts to an update setting `DeletedAt = UtcNow`.
   - Global query filter `e.DeletedAt == null` automatically hides deleted rows. Use `.IgnoreQueryFilters()` only for administrative restores or uniqueness/seed checks.
2. **Foreign Key Integrity:**
   - All foreign keys (31 across the runtime schema) must be configured with `DeleteBehavior.Restrict` (`ON DELETE NO ACTION`).
   - All updates use `ON UPDATE NO ACTION`.
   - Never introduce `Cascade` or `SetNull` in EF Core model configurations.
3. **Unique Constraints:**
   - Always use **filtered unique indexes** for columns that can be reused after soft deletion:
     ```csharp
     b.HasIndex(x => x.Email).IsUnique().HasFilter("[DeletedAt] IS NULL");
     ```
4. **Append-Only Ledgers:**
   - Entities implementing `IAppendOnly` (`ScoreTransaction`, `AuditLog`) can never be modified or deleted. Compensating entries must be written instead.
5. **Shared Configuration:**
   - Do NOT edit `AppDbContext.cs` directly for entity-specific mappings. Add an `IEntityTypeConfiguration<T>` in `backend/src/TeamUp.Infrastructure/Persistence/Configurations/`.
6. **API-Only Backend Rule:**
   - `backend/src/TeamUp.Api` is strictly a JSON REST API (no MVC views or `.cshtml` pages).
   - Server-side templates are allowed **only** for embedded non-UI `.html` transactional emails (`Emails/`), report templates (`Reports/`), and brand assets (`Assets/`) under `backend/src/TeamUp.Infrastructure/Templates/`.

---

## 4. Local Development Commands

```bash
# Restore local dotnet-ef tool manifest (from repo root)
dotnet tool restore

# Build backend with warnings as errors
dotnet build backend/TeamUp.sln

# Run all backend tests (117 Unit + 75 Integration)
dotnet test backend/TeamUp.sln

# Run API locally (uses SQLite fallback automatically if SQL Server is offline in Development)
dotnet run --project backend/src/TeamUp.Api

# Add a migration (M1 only, or coordinated)
dotnet ef migrations add <Name> --project backend/src/TeamUp.Infrastructure --startup-project backend/src/TeamUp.Api --output-dir Persistence/Migrations

# Update local SQL Server database
dotnet ef database update --project backend/src/TeamUp.Infrastructure --startup-project backend/src/TeamUp.Api

# Run React frontend locally (proxies /api, /health, /openapi to http://localhost:5186)
cd frontend && npm install && npm run dev

# Run frontend tests and production build (71 Vitest tests)
cd frontend && npm run test && npm run build

# Run Playwright E2E smoke suite
cd frontend && npm run test:e2e
```
