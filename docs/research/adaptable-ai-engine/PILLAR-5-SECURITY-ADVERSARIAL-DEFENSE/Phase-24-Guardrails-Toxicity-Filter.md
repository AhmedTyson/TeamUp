# Phase 24: Real-Time Guardrails & Content Toxicity Filtering

**Pillar:** V — Security Threat Modeling & Adversarial Defense  
**Module:** `TeamUp.Security.ContentGuard`  
**Target:** Automated Moderation of Bios, Project Titles & Event Descriptions  

---

## 1. Problem & Architectural Threat
In an open university platform where students create public projects, events, and bios via AI assistance, malicious or irresponsible users may attempt to:
- Generate defamatory, sexually explicit, or abusive bios.
- Create offensive event titles or hate-speech project names.
- Attempt academic dishonesty (e.g., using the project board to sell exam questions or plagiarized assignments).

**Requirement:** An automated, zero-latency content moderation guardrail that rejects harmful, toxic, or abusive text on both input and output.

---

## 2. Research & Moderation Architecture

### Dual-Stage Moderation (Fast Lexicon Filter + Semantic Guardrail)
*Citation: Inan et al. (Meta AI, 2023), "Llama Guard: LLM-based Input-Output Security Moderation"; Rebuffi et al. (2024), "NeMo Guardrails Architecture".*

```
User Input Text ──► 1. Exact Profanity / Hate Lexicon (Sub-1ms check)
                         │ Passes
                         ▼
                    2. Semantic Moderation Model (OpenAI Moderation / Llama Guard)
                         │ Scores categories: [Hate: 0.01, Harassment: 0.02, Sexual: 0.00]
                         ▼
                    If all scores < Threshold (0.10): APPROVED
                    If any score >= Threshold (0.10): REJECTED with ValidationException
```

---

## 3. Concrete Implementation (C# / Content Moderation Guard)

```csharp
namespace TeamUp.Infrastructure.Security;

public class ContentModerationGuard(IModerationClient moderationClient) : IContentModerationGuard
{
    private const float HateThreshold = 0.05f;
    private const float HarassmentThreshold = 0.05f;
    private const float SexualThreshold = 0.05f;

    public async Task AssertContentSafeAsync(string text, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        var scores = await moderationClient.AnalyzeToxicityAsync(text, ct);

        if (scores.HateScore >= HateThreshold ||
            scores.HarassmentScore >= HarassmentThreshold ||
            scores.SexualScore >= SexualThreshold)
        {
            throw new ValidationException(
                "Content rejected: The proposed text violates TeamUp university community guidelines regarding harassment, hate speech, or inappropriate language.");
        }
    }
}
```

---

## 4. Failure Modes & Mitigations
- **Leetspeak / Character Substitution ("h@te", "b!tch"):** The classifier uses unicode normalization (NFKC) and unidecode before lexicon checks, destroying obfuscated bypasses.
- **Academic Context False Positives (e.g. "Computer Network Attacks Course"):** Technical/academic terminology is whitelisted in system prompts to prevent blocking legitimate cybersecurity coursework.
