# Phase 30: System Master Runbook & Production Rollout Checklist

**Pillar:** VI — Production Architecture, Observability & Governance  
**Module:** `TeamUp.DevOps.MasterRunbook`  
**Target:** Operational Procedures, Capacity Planning & Production Verification  

---

## 1. Production Capacity & Cost Planning (10,000 Students)

### Workload Assumptions:
- **Active Students:** 10,000
- **Daily Active Users (DAU):** 2,500
- **New Onboarding Requests:** ~200 / day
- **Multimodal Flyer Uploads:** ~30 / day
- **Average Prompt Size:** 350 input tokens, 120 output tokens

### Monthly AI Operating Cost Projection (Dual Provider Setup):
| Operation | Monthly Volume | Model Family | Cost / Unit | Monthly Cost (USD) |
|---|---|---|---|---|
| Conversational Onboarding | 6,000 turns | GPT-4o-mini | $0.0003 / turn | $1.80 |
| Multimodal Flyer Parsing | 900 images | Gemini 1.5 Flash | $0.0025 / image | $2.25 |
| Teammate Vector Matching | 30,000 queries | text-embedding-3-small | $0.00002 / call | $0.60 |
| Semantic Cache Overhead | — | Redis (Memory) | Fixed server cost | $15.00 |
| **Total Projected Monthly Cost** | | | | **~$19.65 / month** |

*Conclusion:* Using modern frontier compact models (`gpt-4o-mini`, `gemini-1.5-flash`), an intent-driven AI platform serving 10,000 university students costs **under $25/month** in cloud inference.

---

## 2. Production Deployment & Verification Checklist

```
[ ] 1. SECRETS & CREDENTIALS
    • Zero hardcoded keys in repository.
    • Production connection string set via ConnectionStrings__TeamUpDb.
    • Production JWT secret (>= 256 bits) set via Jwt__Key.
    • Cloud AI keys configured via environment variables.

[ ] 2. DATABASE & MIGRATION INTEGRITY
    • EF Core migrations up to date: dotnet ef database update.
    • Zero physical cascades verified via sys.foreign_keys check.
    • All 10 filtered unique indexes active (WHERE [DeletedAt] IS NULL).

[ ] 3. SECURITY & RATE LIMITING
    • Rate limiting active on /api/v1/auth/* (60 req/min).
    • Reverse proxy (Cloudflare / NGINX) enforcing 100MB max payload body.
    • PII redaction pipeline verified against Egyptian national IDs.

[ ] 4. OBSERVABILITY & TELEMETRY
    • OpenTelemetry Collector exporting traces to Jaeger / Application Insights.
    • Prometheus /health/live and /health/ready probes configured.
    • Slack / PagerDuty alert webhooks wired for Circuit Breaker trip events.
```

---

## 3. Operational Verification Commands

```bash
# 1. Verify build clean with 0 warnings
dotnet build TeamUp.sln --configuration Release

# 2. Run all unit and integration tests
dotnet test TeamUp.sln --no-build --configuration Release

# 3. Verify zero database cascades in generated migration script
dotnet ef migrations script --project src/TeamUp.Infrastructure --startup-project src/TeamUp.Api -i -o verify-cascades.sql
grep -E "ON DELETE CASCADE|ON DELETE SET NULL" verify-cascades.sql && echo "FAIL: Cascades found!" || echo "PASS: Zero cascades."

# 4. Execute live end-to-end API scenario suite
pwsh -File scripts/test_live_api.ps1
```

---

## 4. Disaster Recovery & Incident Protocol
- **Primary AI Provider Outage:** The gateway automatically fails over to the secondary provider via Polly circuit breaker. No manual operator intervention is required.
- **Runaway Token Consumption:** The semantic token budget service automatically throttles offending IP addresses at 15,000 tokens/day, preventing unexpected cloud billing spikes.
