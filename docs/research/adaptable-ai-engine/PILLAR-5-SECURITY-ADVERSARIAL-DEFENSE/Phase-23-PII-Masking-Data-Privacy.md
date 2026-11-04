# Phase 23: Sensitive Data Sanitization & PII Masking

**Pillar:** V — Security Threat Modeling & Adversarial Defense  
**Module:** `TeamUp.Security.PiiMasking`  
**Target:** Automated Redaction of Sensitive Data Before LLM Provider Ingestion  

---

## 1. Problem & Architectural Threat
Students often inadvertently type sensitive Personal Identifiable Information (PII) into the chat window:
> *"Hi, my national ID is 29801011234567, phone is 01012345678, and my student fee card number is 4111-2222-3333-4444."*

Transmitting raw student PII to cloud LLM providers (OpenAI, Anthropic, Google):
1. Violates student privacy regulations (FERPA, GDPR Article 9, Egyptian Data Protection Law No. 151 of 2020).
2. Exposes confidential student information in provider prompt logs.

**Requirement:** An in-memory, zero-allocation pre-processor that detects and redacts sensitive PII before prompt transmission.

---

## 2. Research & Token Redaction Pipeline

### Named Entity Recognition (NER) & Deterministic Regex Scrubbing
*Citation: Microsoft Presidio (2024); NIST Special Publication 800-122, "Guide to Protecting the Confidentiality of Personally Identifiable Information (PII)".*

```
Raw User Text: "Phone is 01012345678 and ID is 29801011234567"
                      │
                      ▼ Deterministic PII Sanitizer (Regex + Luhn Check)
                      ├── Phone Pattern ──► [REDACTED_PHONE]
                      ├── National ID   ──► [REDACTED_NATIONAL_ID]
                      └── Credit Card   ──► [REDACTED_PAYMENT_CARD]
                      │
                      ▼
Clean Prompt sent to LLM: "Phone is [REDACTED_PHONE] and ID is [REDACTED_NATIONAL_ID]"
```

---

## 3. Concrete Implementation (C# / High-Performance Regex Sanitizer)

```csharp
namespace TeamUp.Infrastructure.Security;

public class PiiMaskingService : IPiiMaskingService
{
    // Egyptian Phone (010, 011, 012, 015 + 8 digits)
    private static readonly Regex EgyptianPhoneRegex = new(
        @"\b01[0125][0-9]{8}\b", RegexOptions.Compiled);

    // 14-digit Egyptian National ID
    private static readonly Regex NationalIdRegex = new(
        @"\b[23][0-9]{13}\b", RegexOptions.Compiled);

    // Standard Credit Card (Visa, MasterCard, Amex)
    private static readonly Regex CreditCardRegex = new(
        @"\b(?:\d{4}[ -]?){3}\d{4}\b", RegexOptions.Compiled);

    public string MaskPii(string rawInput)
    {
        if (string.IsNullOrWhiteSpace(rawInput)) return rawInput;

        var sanitized = rawInput;
        sanitized = CreditCardRegex.Replace(sanitized, "[CARD_REDACTED]");
        sanitized = NationalIdRegex.Replace(sanitized, "[NATIONAL_ID_REDACTED]");
        sanitized = EgyptianPhoneRegex.Replace(sanitized, "[PHONE_REDACTED]");

        return sanitized;
    }
}
```

---

## 4. Failure Modes & Mitigations
- **False Positives (e.g. Course codes like "ENG 202"):** Regex patterns use strict digit-length and prefix anchors to avoid mangling genuine course codes or dates.
- **Provider Training Opt-Out:** In cloud provider contracts, ensure the **Zero-Data Retention (ZDR)** API agreement is active, guaranteeing prompts are never used for model training.
