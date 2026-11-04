# Phase 12: Project & Team Auto-Drafting from Conversational Pitch

**Pillar:** III — Autonomous Entity Synthesis & Execution  
**Module:** `TeamUp.AiGateway.ProjectDrafting`  
**Target:** Converting Natural Language Pitches into Complete Project Aggregates  

---

## 1. Problem & Architectural Challenge
Students often have project ideas (graduation projects, hackathon teams) but struggle to articulate structured specifications:
> *"I want to build an autonomous drone for crop disease detection using computer vision on Raspberry Pi. Need a hardware person and a PyTorch dev."*

To create a recruiting project, the system must:
1. Synthesize an engaging, professional `Title` and technical `Description`.
2. Determine realistic `MaxTeamSize` (default: 4).
3. Map technical needs (*"PyTorch"*, *"Raspberry Pi"*, *"Computer Vision"*) to exact database `SkillID`s in the platform catalog.
4. Auto-enroll the creator as `ProjectMember` with role `"Team Lead"`.

---

## 2. Extraction & Skill Mapping Architecture

```
User Pitch: "Autonomous drone for crop disease detection..."
                         │
                         ▼ LLM Structured Generation
┌────────────────────────────────────────────────────────┐
│ Synthesized Project Specification                      │
│ • Title: "AeroCrop: Autonomous Drone Disease Detection"│
│ • Description: "IoT & Computer Vision system..."       │
│ • Recommended Team Size: 4                             │
│ • Identified Competencies:                             │
│   ["Computer Vision", "PyTorch", "Embedded C++", "IoT"]│
└──────────────────────────┬─────────────────────────────┘
                           │
                           ▼ Semantic Catalog Grounding
┌────────────────────────────────────────────────────────┐
│ Skill Resolver Pipeline (Against platform Skill table) │
│ • "Computer Vision"  ──► Match: SkillID = 22 (Category: AI)   │
│ • "PyTorch"          ──► Match: SkillID = 11 (Category: AI)   │
│ • "Embedded C++"     ──► Match: SkillID = 1  (Category: C#)   │
└────────────────────────────────────────────────────────┘
```

---

## 3. Concrete Implementation (C# / Project Drafting Engine)

```csharp
namespace TeamUp.Application.Ai.ProjectDrafting;

public sealed record SynthesizeProjectDraftRequest(string PitchPrompt);

public sealed record ProjectDraftResultDto(
    string Title,
    string Description,
    int RecommendedTeamSize,
    List<ProjectRequiredSkillDraftDto> RequiredSkills,
    List<string> SuggestedBacklogTasks
);

public class ProjectDraftingService(
    AppDbContext db,
    ILlmClient llmClient,
    ISkillDisambiguationService skillResolver) : IProjectDraftingService
{
    public async Task<ProjectDraftResultDto> DraftProjectAsync(string pitch, CancellationToken ct)
    {
        var rawSpec = await llmClient.ExtractProjectSpecAsync(pitch, ct);

        // Ground raw skill names against platform master catalog
        var resolvedSkills = new List<ProjectRequiredSkillDraftDto>();
        foreach (var skillName in rawSpec.IdentifiedSkills)
        {
            var match = await skillResolver.ResolveSkillAsync(skillName, ct);
            if (match is not null)
            {
                resolvedSkills.Add(new ProjectRequiredSkillDraftDto(
                    SkillId: match.SkillId,
                    SkillName: match.Name,
                    RequiredLevel: ProficiencyLevel.Intermediate,
                    IsMandatory: true
                ));
            }
        }

        return new ProjectDraftResultDto(
            Title: rawSpec.SuggestedTitle,
            Description: rawSpec.RefinedDescription,
            RecommendedTeamSize: Math.Clamp(rawSpec.SuggestedTeamSize, 2, 8),
            RequiredSkills: resolvedSkills,
            SuggestedBacklogTasks: rawSpec.StarterTasks
        );
    }
}
```

---

## 4. Failure Modes & Mitigations
- **Overestimating Team Size:** Model might suggest a team of 15 for a simple web app. Deterministic validator clamps `TeamSize` between 2 and 6 for student course projects.
- **Novel / Unmatched Skills:** If a user specifies a rare library (e.g. *"LangGraph"*), it is added to a candidate skill suggestion queue for PlatformAdmin review, without breaking project creation.
