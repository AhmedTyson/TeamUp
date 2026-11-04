# Phase 25: Denial-of-Wallet & Semantic Token Rate Limiting

**Pillar:** V — Security Threat Modeling & Adversarial Defense  
**Module:** `TeamUp.Security.RateLimiting`  
**Target:** Financial & Compute Resource Protection Against Exhaustion Attacks  

---

## 1. Problem & Architectural Threat
**Denial-of-Wallet (DoW)** attacks exploit the pay-per-token nature of frontier AI models:
- An attacker writes a Python script that spams the landing page hero prompt 50,000 times with long, token-dense essays.
- At $0.01 per turn, an unthrottled endpoint generates a **$500 cloud bill within 20 minutes**, while exhausting backend thread pools.

**Requirement:** Multi-layer compute and financial rate limiting that protects platform cloud budgets while preserving a frictionless experience for legitimate students.

---

## 2. Research & Multi-Tier Throttling Architecture

### Token-Aware Rate Limiting & Semantic Caching
*Citation: OWASP LLM04 (2025), "Model Denial of Service"; Bang et al. (2023), "GPTCache: An Open-Source Semantic Cache for LLM Applications".*

```
Incoming Hero Request ──► 1. IP Fixed-Window Rate Limiter (Max 15 req/min per IP)
                               │ Passes
                               ▼
                          2. Semantic Cache Check (Redis Cosine Similarity > 0.98)
                               │ If match found: Return cached result (Cost: $0.00, Latency: 4ms!)
                               ▼ If cache miss:
                          3. User / Session Token Budget Check
                               │ (Guest limit: 10,000 tokens/day; Student limit: 100,000 tokens/day)
                               ▼
                          4. Execute LLM Call & Deduct Tokens from Budget
```

---

## 3. Concrete Implementation (C# / Semantic Cache & Token Limiter)

```csharp
namespace TeamUp.Infrastructure.Security;

public class SemanticTokenBudgetService(
    IDistributedCache cache,
    ISemanticCacheClient semanticCache) : ISemanticTokenBudgetService
{
    private const int MaxGuestDailyTokens = 15000;

    public async Task<string?> TryGetCachedResponseAsync(string prompt, CancellationToken ct)
    {
        // Cosine similarity check over pre-computed prompt embeddings
        return await semanticCache.FindSimilarResultAsync(prompt, similarityThreshold: 0.98f, ct);
    }

    public async Task AssertBudgetAvailableAsync(string clientIp, int estimatedTokens, CancellationToken ct)
    {
        var key = $"budget_tokens:{clientIp}:{DateTime.UtcNow:yyyyMMdd}";
        var currentUsedStr = await cache.GetStringAsync(key, ct);
        int currentUsed = int.TryParse(currentUsedStr, out var v) ? v : 0;

        if (currentUsed + estimatedTokens > MaxGuestDailyTokens)
        {
            throw new ValidationException(
                "Daily AI interaction quota reached for this connection. Please log in or try again tomorrow.");
        }

        // Increment usage
        await cache.SetStringAsync(key, (currentUsed + estimatedTokens).ToString(), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
        }, ct);
    }
}
```

---

## 4. Failure Modes & Mitigations
- **Distributed Botnets (Rotating IPs):** In production, Cloudflare Turnstile (invisible CAPTCHA) activates automatically when an IP's Cloudflare Threat Score rises above 15.
- **Cache Poisoning:** Semantic cache entries are strictly scoped by entity tenant/user where personalized data is present; only public lookups are shared across guests.
