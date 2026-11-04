# Phase 11: Document-to-Event Synthesis Pipeline

**Pillar:** III — Autonomous Entity Synthesis & Execution  
**Module:** `TeamUp.AiGateway.EventSynthesis`  
**Target:** Automated Event & Multi-Session Relational Entity Generation  

---

## 1. Problem & Architectural Challenge
Creating a full academic workshop on the platform requires filling multiple relational rows:
1. `Event` (Title, Description, Location, Capacity, Dates, `OrganizationID`)
2. Multiple `EventSession` records (Title, StartTime, EndTime, Room, SessionNo)
3. Skill associations for matchmaking.

When an organizer drops a flyer or types:
> *"Host an AI Hackathon on Dec 1st-2nd at Hall B, 100 capacity, sponsored by IEEE."*

The system must autonomously synthesize these interconnected relational entities without foreign key violations or data loss.

---

## 2. Relational Mapping & Transformation Flow

```
Input: Text / Flyer Image
           │
           ▼ Multimodal Extraction (Phase 09)
┌────────────────────────────────────────────────────────┐
│ Intermediate Synthesized Draft DTO                     │
│ • Title: "AI Hackathon 2026"                           │
│ • OrganizationMention: "IEEE" ➔ Resolved to OrgID = 3  │
│ • Sessions: [Day 1, Day 2]                             │
└──────────────────────────┬─────────────────────────────┘
                           │
                           ▼ Relational Mapping (EF Core)
┌────────────────────────────────────────────────────────┐
│ AppDbContext Transaction Boundary                      │
│ 1. Validate Organization ownership (User is OrgAdmin)  │
│ 2. Insert Event (OrganizationId = 3, Capacity = 100)   │
│ 3. For each session: Insert EventSession (SessionNo++) │
│ 4. Commit ACID Transaction                             │
└────────────────────────────────────────────────────────┘
```

---

## 3. Concrete Implementation (C# / Event Synthesis Orchestrator)

```csharp
namespace TeamUp.Application.Ai.EventSynthesis;

public sealed record SynthesizeEventRequest(
    string RawTextOrPrompt,
    string? UploadedFlyerUrl,
    int OrganizationId
);

public class EventSynthesisService(
    AppDbContext db,
    IFlyerExtractionService flyerService,
    IAcademicEntityResolver resolver) : IEventSynthesisService
{
    public async Task<EventDraftPreviewDto> SynthesizeAsync(
        SynthesizeEventRequest req, 
        int organizerStudentId, 
        CancellationToken ct)
    {
        // 1. Verify caller is an active OrgAdmin for the target organization
        var isAuthorized = await db.OrganizationMembers
            .AnyAsync(m => m.OrganizationId == req.OrganizationId && 
                           m.StudentId == organizerStudentId && 
                           m.Role == "President" || m.Role == "Head of PR" || m.Role == "Admin", ct);

        if (!isAuthorized)
            throw new ForbiddenException("Only authorized Organization Admins can synthesize events.");

        // 2. Extract draft schema via Multimodal Vision or LLM
        EventDraftDto draft;
        if (!string.IsNullOrWhiteSpace(req.UploadedFlyerUrl))
        {
            // Extract from flyer pixels
            draft = await flyerService.ExtractFromBlobUrlAsync(req.UploadedFlyerUrl, ct);
        }
        else
        {
            // Extract from prompt text
            draft = await ExtractFromTextPromptAsync(req.RawTextOrPrompt, ct);
        }

        // 3. Construct preview model for Human-in-the-Loop review
        return new EventDraftPreviewDto(
            OrganizationId: req.OrganizationId,
            Title: draft.Title,
            Description: draft.Description,
            Type: draft.Type,
            Location: draft.Location,
            StartDate: draft.StartDate,
            EndDate: draft.EndDate,
            Capacity: draft.Capacity ?? 50,
            Sessions: draft.Sessions.Select((s, idx) => new EventSessionDraftDto(
                SessionNo: idx + 1,
                Title: s.Title,
                StartTime: s.StartTime,
                EndTime: s.EndTime,
                Room: s.Room
            )).ToList(),
            RequiredSkills: draft.ExtractedSkills
        );
    }
}
```

---

## 4. Failure Modes & Mitigations
- **Overlapping Session Times:** Validation pipeline asserts `Session[i].EndTime <= Session[i+1].StartTime`.
- **Undefined Organization in Text:** If prompt does not mention which club the user represents, the service checks the student's active `OrganizationMember` memberships. If they only belong to 1 organization, it auto-assigns it; if multiple, it prompts for selection.
