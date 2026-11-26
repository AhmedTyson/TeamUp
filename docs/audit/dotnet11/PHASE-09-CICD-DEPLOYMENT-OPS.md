# Phase 09 Audit — CI/CD, MonsterASP.NET Deployment & Ops

**Date:** 2026-11-26  
**Scope:** `.github/workflows/ci.yml`, `.github/workflows/deploy-monsterasp.yml`, `scripts/publish-monsterasp.ps1`, `scripts/test_live_api.ps1`, `vercel.json`, `frontend/vercel.json`, `netlify.toml`, `appsettings*.json`, `backend/.env*`.

---

## 1. Key Findings & Remediation Plan

| ID | Severity | Location | Finding | Remediation (Apply Phase 9) |
|---|---|---|---|---|
| OPS-01 | **P1** | `.github/workflows/ci.yml:11,21,72,83`, `deploy-monsterasp.yml:13,43,47`, `scripts/publish-monsterasp.ps1:3,69` | All CI/CD workflows and publish scripts hardcode `dotnet-version: '10.0.x'` and `.NET 10` labels. | Update CI & MonsterASP workflows and `publish-monsterasp.ps1` to `11.0.x` / `.NET 11` (MonsterASP.NET officially supports .NET 11 In-Process hosting on Windows Server 2025). Preserve zero-cascade EF script verification in `ci.yml:35-42`. |
| OPS-02 | **P1** | `scripts/publish-monsterasp.ps1:48-57` vs `deploy-monsterasp.yml:56-65` | `publish-monsterasp.ps1` does not validate `SeedImages\cover-01..10.png` in publish output and omits non-Windows `runtimes/` pruning (`win`, `win-x64`, `win-x86` only), producing oversized manual deployment ZIPs. | Port `runtimes/` pruning and add `SeedImages\cover-01.png`..`cover-10.png` presence check to `scripts/publish-monsterasp.ps1`. Verify via live `dotnet publish -c Release`. |
| OPS-03 | **P2** | `TeamUp.Api.csproj:10` | `<ServerGarbageCollection>false</ServerGarbageCollection>` keeps memory below MonsterASP.NET's 256 MB Free-tier ceiling. | Preserve explicitly across `.NET 11` upgrade. |
| OPS-04 | **P2** | `appsettings.Production.example.json:10-14,34-35` | Mixes generic placeholders (`your-frontend.pages.dev`) with hardcoded `https://teamup-ieee.vercel.app`. | Keep clear documentation/comments aligning placeholders cleanly. |
| OPS-05 | **P3** | `scripts/test_live_api.ps1:151,331` | References `ASP.NET Core 10 Web API` and reads `Seed:AdminPassword` only from `dotnet user-secrets`. | Update label to `ASP.NET Core 11` and fall back to `$env:Seed__AdminPassword` / `DevelopmentSeedPassword2026!`. |
