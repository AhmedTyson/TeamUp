# Phase 27: OpenTelemetry Instrumentation & Token Usage Ledger

**Pillar:** VI — Production Architecture, Observability & Governance  
**Module:** `TeamUp.Observability.AiTelemetry`  
**Target:** Granular Cost Auditing, Tracing, and Latency Monitoring  

---

## 1. Problem & Architectural Challenge
AI components can degrade silently:
- Provider latency spikes from 300ms to 4000ms.
- Prompt token usage creeps up unnoticed due to accumulating conversation history.
- Cloud API bills balloon without visibility into which user or feature drove the cost.

**Requirement:** Full OpenTelemetry distributed tracing and metrics instrumentation logging **Tokens, Latency, Cost (USD), and Model Identifiers** on every turn.

---

## 2. Research & OpenTelemetry Semantic Conventions

### GenAI Semantic Conventions (CNCF / OpenTelemetry v1.28+)
*Citation: OpenTelemetry Semantic Conventions for Generative AI Systems (2024); W3C Distributed Tracing Standards.*

Every AI invocation generates an OpenTelemetry Span with standard attributes:
- `gen_ai.system`: `"openai"` / `"anthropic"` / `"google"`
- `gen_ai.request.model`: `"gpt-4o-mini-2024-07-18"`
- `gen_ai.usage.prompt_tokens`: `142`
- `gen_ai.usage.completion_tokens`: `85`
- `gen_ai.response.latency_ms`: `342`
- `teamup.student_id`: `14`
- `teamup.cost_usd`: `0.00045`

```
Trace [POST /api/v1/auth/register]
 │
 ├──► Span [ValidationFilter] (2ms)
 ├──► Span [GenAI: SlotExtraction] (280ms, 185 tokens, $0.0003)
 ├──► Span [Disambiguation: CatalogLookup] (14ms)
 └──► Span [SQL: InsertStudent] (8ms)
```

---

## 3. Concrete Implementation (C# / ActivitySource & Meter)

```csharp
namespace TeamUp.Infrastructure.Observability;

public static class AiTelemetry
{
    public static readonly ActivitySource ActivitySource = new("TeamUp.AiGateway", "1.0.0");
    public static readonly Meter Meter = new("TeamUp.AiGateway", "1.0.0");

    private static readonly Counter<long> TokenCounter = Meter.CreateCounter<long>(
        "gen_ai.client.token.usage", "tokens", "Total tokens consumed by AI workloads");

    private static readonly Histogram<double> LatencyHistogram = Meter.CreateHistogram<double>(
        "gen_ai.client.operation.duration", "ms", "Latency of AI operations");

    public static void RecordUsage(string model, int promptTokens, int completionTokens, double durationMs)
    {
        TokenCounter.Add(promptTokens, new KeyValuePair<string, object?>("type", "input"), new("model", model));
        TokenCounter.Add(completionTokens, new KeyValuePair<string, object?>("type", "output"), new("model", model));
        LatencyHistogram.Record(durationMs, new KeyValuePair<string, object?>("model", model));
    }
}
```

---

## 4. Failure Modes & Mitigations
- **Telemetry Overhead:** Tracing spans use zero-allocation `Activity` structs from `System.Diagnostics`. High-volume spans are sampled at 10% in production, while failed spans are sampled at 100%.
- **PII in Trace Attributes:** OpenTelemetry attributes capture token counts and prompt IDs, **never** raw prompt strings containing student names or passwords.
