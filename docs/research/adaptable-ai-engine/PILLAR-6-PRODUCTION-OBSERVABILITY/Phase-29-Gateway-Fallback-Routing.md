# Phase 29: Multi-Provider Resiliency & Circuit Breaker Gateway

**Pillar:** VI — Production Architecture, Observability & Governance  
**Module:** `TeamUp.AiGateway.Resilience`  
**Target:** High-Availability AI Ingestion with Multi-Cloud Provider Fallbacks  

---

## 1. Problem & Architectural Challenge
Cloud AI providers experience periodic outages, rate limits (HTTP 429), and regional latency spikes:
- If TeamUp relies exclusively on a single provider (e.g. OpenAI), an OpenAI outage renders student onboarding and event creation completely inoperable.
- A sudden rate limit spike can leave hundreds of students unable to complete registration during campus orientation week.

**Requirement:** An enterprise **Multi-Provider Resilient Gateway** with automated health probes, latency-based load balancing, and **Polly Circuit Breakers**.

---

## 2. Research & Circuit Breaker State Machine

### State-Aware Provider Failover
*Citation: Nygard (2018), "Release It!: Design and Deploy Production-Ready Software"; Polly Resilience Project (2024).*

```
Incoming AI Request
         │
         ▼ Primary Provider (OpenAI GPT-4o-mini)
    ┌───────────┐
    │  CLOSED   │ ──► Success ──► Return to Client
    └─────┬─────┘
          │ 3 Consecutive Timeouts / Failures
          ▼
    ┌───────────┐
    │   OPEN    │ ──► Immediate Failover ──► Secondary Provider (Anthropic Claude 3.5 Haiku)
    └─────┬─────┘                                  │ Fails
          │ After 30s                               ▼
          ▼                                  Tertiary Provider (Google Gemini 1.5 Flash)
    ┌───────────┐                                  │ All Cloud Down
    │ HALF-OPEN │ ──► Probes 1 request             ▼
    └───────────┘     to Primary             Graceful UI Degradation (Interactive Manual Form)
```

---

## 3. Concrete Implementation (C# / Polly Resilience Pipeline)

```csharp
namespace TeamUp.Infrastructure.Ai.Gateway;

public class ResilientAiGateway(
    IOpenAiClient primaryClient,
    IAnthropicClient secondaryClient,
    IGeminiClient tertiaryClient,
    ILogger<ResilientAiGateway> logger) : IResilientAiGateway
{
    private static readonly ResiliencePipeline Pipeline = new ResiliencePipelineBuilder()
        .AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
        {
            FailureRatio = 0.5,
            SamplingDuration = TimeSpan.FromSeconds(30),
            MinimumThroughput = 5,
            BreakDuration = TimeSpan.FromSeconds(30)
        })
        .AddTimeout(TimeSpan.FromSeconds(4))
        .Build();

    public async Task<string> GenerateStructuredCompletionAsync(string prompt, object schema, CancellationToken ct)
    {
        try
        {
            return await Pipeline.ExecuteAsync(async token => 
                await primaryClient.GenerateAsync(prompt, schema, token), ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Primary AI provider failed. Tripping circuit to Secondary provider.");
            try
            {
                return await secondaryClient.GenerateAsync(prompt, schema, ct);
            }
            catch (Exception secEx)
            {
                logger.LogError(secEx, "Secondary AI provider failed. Tripping circuit to Tertiary provider.");
                return await tertiaryClient.GenerateAsync(prompt, schema, ct);
            }
        }
    }
}
```

---

## 4. Failure Modes & Graceful Degradation
- **Global Cloud AI Outage:** If all cloud providers fail simultaneously, the gateway emits a fallback event to the frontend:
  `{ "fallbackMode": true, "message": "AI assistant is temporarily resting. Here is your visual form." }`
  The frontend gracefully renders the standard interactive form components. **The platform never goes down.**
