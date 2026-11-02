# TeamUp — Contribution & Team Workflow Guidelines

## 1. Team Ownership Split (4 Members)

| Member | Focus Area | Entities / Tables Owned |
|---|---|---|
| **M1** (Tech Lead) | Architecture, Auth, Identity | `University`, `Faculty`, `Student`, `StudentInterest`, `StudentProfile`, Base Entities, CI |
| **M2** | Skills, Projects & Teams | `Skill`, `StudentSkill`, `Project`, `ProjectRequiredSkill`, `ProjectMember`, `ProjectApplication`, `ProjectTask` |
| **M3** | Student Activities & Events | `Organization`, `OrganizationMember`, `MembershipApplication`, `Event`, `EventRegistration`, `EventSession`, `SessionAttendance` |
| **M4** | Gamification, Alerts & Docs | `ScoreTransaction`, `Badge`, `StudentBadge`, `Notification`, OpenAPI / Scalar, Postman, API Docs |

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
  - CI must pass (build clean, zero warnings, all tests pass, zero SQL cascades).
  - Use squash-and-merge with a clean conventional commit message (`feat: ...`, `fix: ...`).

---

## 3. Database & EF Core Rules

1. **Soft Delete Standard:**
   - Every entity except `ScoreTransaction` implements `ISoftDelete` (`IsDeleted`, `DeletedAt`).
   - Physical deletion (`DELETE FROM`) is prohibited in application code. Use `db.Remove(...)` which our `SoftDeleteInterceptor` converts to an update, or call soft-delete methods directly.
   - Global query filter `!e.IsDeleted` automatically hides deleted rows. Use `.IgnoreQueryFilters()` only for administrative restores.
2. **Foreign Key Integrity:**
   - All foreign keys must be configured with `DeleteBehavior.Restrict` (`ON DELETE NO ACTION`).
   - All updates use `ON UPDATE NO ACTION`.
   - Never introduce `Cascade` or `SetNull` in EF Core model configurations.
3. **Unique Constraints:**
   - Always use **filtered unique indexes** for columns that can be reused after deletion:
     ```csharp
     b.HasIndex(x => x.Email).IsUnique().HasFilter("[IsDeleted] = 0");
     ```
4. **Append-Only Ledgers:**
   - Entities implementing `IAppendOnly` (such as `ScoreTransaction`) can never be modified or deleted. Compensating entries must be written instead.
5. **Shared Configuration:**
   - Do NOT edit `AppDbContext.cs` directly for entity-specific mappings. Add an `IEntityTypeConfiguration<T>` in `src/TeamUp.Infrastructure/Persistence/Configurations/`.

---

## 4. Local Development Commands

```bash
# Build with warnings as errors
dotnet build TeamUp.sln

# Run all tests
dotnet test TeamUp.sln

# Run API locally
dotnet run --project src/TeamUp.Api

# Add a migration (M1 only, or coordinated)
dotnet ef migrations add <Name> --project src/TeamUp.Infrastructure --startup-project src/TeamUp.Api --output-dir Persistence/Migrations

# Update local SQL Server database
dotnet ef database update --project src/TeamUp.Infrastructure --startup-project src/TeamUp.Api
```
