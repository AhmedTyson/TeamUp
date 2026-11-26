# Phase 01 Audit — SDK, Packages, Solution & Build Architecture (.NET 11)

**Date:** 2026-11-26  
**Scope:** `global.json`, `dotnet-tools.json`, `backend/TeamUp.sln`, `Directory.Build.props`, `Directory.Packages.props`, all 6 `.csproj` files.

---

## 1. Current Baseline Inventory

| Artifact | Current State | Target (.NET 11) |
|---|---|---|
| Installed Local SDKs | `8.0.319`, `9.0.321`, `10.0.404` | Install `11.0.100` (`Microsoft.DotNet.SDK.11`) |
| `global.json` | **Missing** (no deterministic SDK pin) | Add `global.json` pinning `11.0.100` (`rollForward: latestFeature`) |
| `dotnet-tools.json` | `dotnet-ef` `10.0.14` | Upgrade `dotnet-ef` to `11.0.0` |
| `backend/Directory.Build.props` | **Missing** (duplicated TFMs/Nullable/ImplicitUsings across 6 `.csproj` files) | Centralize `TargetFramework=net11.0`, `LangVersion=15.0`, `ImplicitUsings=enable`, `Nullable=enable` |
| `backend/Directory.Packages.props` | **Missing** (inconsistent `10.0.14` vs `10.0.*` wildcards across `.csproj` files) | Enable Central Package Management (`ManagePackageVersionsCentrally=true`) |
| `TeamUp.Api.csproj` | `net10.0`, `ServerGarbageCollection=false` (good for MonsterASP 256 MB RAM free tier), `Microsoft.AspNetCore.*` `10.0.14` | `net11.0` via `Directory.Build.props`; packages `11.0.0` |
| `TeamUp.Application.csproj` | `net10.0`, `FluentValidation` `11.*`, `Microsoft.EntityFrameworkCore.*` `10.0.14`, `Microsoft.Extensions.Identity.Core` `10.0.*` | `net11.0`; EF Core & Identity `11.0.0` (keep `FluentValidation` `11.*` for stable license/API) |
| `TeamUp.Domain.csproj` | `net10.0`, zero external dependencies | `net11.0` (clean) |
| `TeamUp.Infrastructure.csproj` | `net10.0`, EF Core SqlServer/Sqlite/Design `10.0.14`, `QuestPDF 2026.9.2`, `System.IdentityModel.Tokens.Jwt 8.*`, `System.Security.Cryptography.Pkcs 10.0.*` | `net11.0`; EF Core `11.0.0`, `System.Security.Cryptography.Pkcs 11.0.0` |
| Test `.csproj` files | `Microsoft.AspNetCore.Mvc.Testing 10.0.14`, `Microsoft.EntityFrameworkCore.Sqlite 10.0.14 / 10.0.*`, `xunit 2.9.3`, `Microsoft.NET.Test.Sdk 17.14.1` | `net11.0`; `11.0.0` Microsoft test/EF packages |

---

## 2. Key Findings & Recommendations

1. **[P1] Missing `global.json` & Central Package Management (CPM):**
   - **Finding:** `TeamUp.Application.csproj` and `TeamUp.IntegrationTests.csproj` mix pinned `10.0.14` with wildcard `10.0.*`, risking non-deterministic transitive graph skew between local and CI builds.
   - **Action (Apply Phase 1):** Introduce `global.json`, `backend/Directory.Build.props`, and `backend/Directory.Packages.props`, stripping inline `Version=` attributes and duplicated `<TargetFramework>` tags from all 6 `.csproj` files.
2. **[P1] .NET 11 SDK & Official NuGet Package Upgrades:**
   - **Finding:** `.NET 11.0.100` SDK and `11.0.0` stable NuGet packages (`Microsoft.EntityFrameworkCore*`, `Microsoft.AspNetCore.OpenApi`, `Microsoft.AspNetCore.Authentication.JwtBearer`, `Microsoft.AspNetCore.Mvc.Testing`, `dotnet-ef`) are published and available.
   - **Action (Apply Phase 1):** Install `.NET 11 SDK`, bump all Microsoft/EF/ASP.NET Core packages and `dotnet-ef` to `11.0.0`.
3. **[P2] Runtime Async & Memory Footprint Tuning:**
   - **Finding:** `TeamUp.Api.csproj` sets `<ServerGarbageCollection>false</ServerGarbageCollection>` specifically for MonsterASP.NET's constrained memory tier (256 MB RAM). Under `.NET 11`, Runtime Async is enabled for `net11.0` targets, reducing async state-machine allocations without sacrificing low-memory GC behavior.
   - **Action (Apply Phase 1):** Preserve `<ServerGarbageCollection>false</ServerGarbageCollection>` in `TeamUp.Api.csproj`, verify zero warnings under `net11.0` / `C# 15`, and keep `SeedImages\*.png` `<CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>`.
4. **[P2] OpenAPI Description & `Microsoft.OpenApi` 3.x Check:**
   - **Finding:** `Program.cs` line 92 hardcodes `"(.NET 10)"` in `document.Info.Description` and uses `OpenApiComponents` / `IOpenApiSecurityScheme` in `AddDocumentTransformer`.
   - **Action (Apply Phase 1 & 4):** Update OpenAPI description to `.NET 11` and verify `Microsoft.AspNetCore.OpenApi 11.0.0` (`OpenAPI 3.2`) compiles and emits valid JSON.
