# Phase 02: Non-Linear Finite State Slot-Filling Engine

**Pillar:** I — Conversational Onboarding & Intent Capture  
**Module:** `TeamUp.AiGateway.Conversation`  
**Target:** Dynamic, Non-Linear Slot Extraction for Student Registration  

---

## 1. Problem & Architectural Challenge
Traditional form-filling bots ask robotic, sequential questions:
> Bot: *"What is your first name?"*  
> User: *"I'm Ahmed Tyson, studying at Cairo Uni."*  
> Bot: *"What is your last name?"* (ignoring that the user already answered it).

Users speak non-linearly. A user might say:
> *"My name is Ahmed Tyson, 3rd year software engineering at Cairo University, email is ahmed@cu.edu.eg, and I love robotics and computer vision."*

The engine must extract **all 7 required registration slots simultaneously in one turn**, check what is missing, and only prompt for the remaining slots.

---

## 2. Research & Mathematical Grounding

### The Slot-Filling FSM (Finite State Machine)
*Citation: Pieraccini et al., "Spoken Language Understanding"; Williams et al., "Partially Observable Markov Decision Processes for Spoken Dialog Systems".*

Let $S = \{s_{\text{first}}, s_{\text{last}}, s_{\text{email}}, s_{\text{year}}, s_{\text{uni}}, s_{\text{fac}}, s_{\text{interests}}\}$ be the set of target slots.  
Let state vector $V \in \{0, 1\}^7$ where $V_i = 1$ when slot $s_i$ is filled and validated.

```
                  ┌──────────────────────────────┐
                  │ INITIAL STATE (V = [0,0...0])│
                  └──────────────┬───────────────┘
                                 │ User Natural Language
                                 ▼
                  ┌──────────────────────────────┐
                  │ EXTRACTOR (Probabilistic LLM)│
                  └──────────────┬───────────────┘
                                 │ Structured Slots
                                 ▼
                  ┌──────────────────────────────┐
                  │ VALIDATOR (Deterministic)    │
                  └──────────────┬───────────────┘
                                 │ Validated Subset
                                 ▼
    ┌────────────────────────────────────────────────────────┐
    │ STATE UPDATE: V_new = V_old ∪ V_extracted              │
    ├────────────────────────────┬───────────────────────────┤
    │ If |V_new| < 7:            │ If |V_new| == 7:          │
    │ Generate targeted question │ Transition to             │
    │ for unfilled slots ONLY.   │ PREVIEW & CONFIRM STATE.  │
    └────────────────────────────┴───────────────────────────┘
```

---

## 3. Data Contracts & Concrete Implementation

### Slot Definition
```csharp
namespace TeamUp.Application.Ai.Conversation;

public sealed record OnboardingSlots(
    string? FirstName = null,
    string? LastName = null,
    string? Email = null,
    int? AcademicYear = null,
    string? UniversityName = null,
    string? FacultyName = null,
    List<string>? Interests = null
)
{
    public bool IsComplete =>
        !string.IsNullOrWhiteSpace(FirstName) &&
        !string.IsNullOrWhiteSpace(LastName) &&
        !string.IsNullOrWhiteSpace(Email) &&
        AcademicYear is >= 1 and <= 7 &&
        !string.IsNullOrWhiteSpace(UniversityName) &&
        !string.IsNullOrWhiteSpace(FacultyName);

    public IEnumerable<string> MissingSlots
    {
        get
        {
            if (string.IsNullOrWhiteSpace(FirstName)) yield return "first name";
            if (string.IsNullOrWhiteSpace(LastName)) yield return "last name";
            if (string.IsNullOrWhiteSpace(Email)) yield return "university email";
            if (AcademicYear is not (>= 1 and <= 7)) yield return "academic year (1-7)";
            if (string.IsNullOrWhiteSpace(UniversityName)) yield return "university";
            if (string.IsNullOrWhiteSpace(FacultyName)) yield return "faculty/college";
        }
    }
}
```

### Deterministic State Machine Engine
```csharp
public class OnboardingStateMachine
{
    public (OnboardingSlots Merged, string NextQuestion) ProcessTurn(
        OnboardingSlots current, 
        OnboardingSlots extracted)
    {
        var merged = new OnboardingSlots(
            FirstName: extracted.FirstName ?? current.FirstName,
            LastName: extracted.LastName ?? current.LastName,
            Email: extracted.Email ?? current.Email,
            AcademicYear: extracted.AcademicYear ?? current.AcademicYear,
            UniversityName: extracted.UniversityName ?? current.UniversityName,
            FacultyName: extracted.FacultyName ?? current.FacultyName,
            Interests: (current.Interests ?? []).Union(extracted.Interests ?? []).Distinct().ToList()
        );

        if (merged.IsComplete)
        {
            return (merged, "Awesome! I've prepared your student profile draft. Please review and confirm below.");
        }

        var missing = merged.MissingSlots.ToList();
        var question = $"Thanks! Could you also let me know your {string.Join(" and ", missing)}?";

        return (merged, question);
    }
}
```

---

## 4. Failure Modes & Mitigations
- **User provides ambiguous academic year ("senior", "junior"):** System prompt maps academic levels deterministically (`Freshman` ➔ 1, `Sophomore` ➔ 2, `Junior` ➔ 3, `Senior` ➔ 4).
- **User changes their mind ("Actually, my email is test@other.edu"):** Newer non-null extractions overwrite existing slots without restarting the entire state machine.
