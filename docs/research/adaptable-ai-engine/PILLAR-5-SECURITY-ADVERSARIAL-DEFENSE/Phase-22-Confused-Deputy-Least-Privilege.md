# Phase 22: Confused Deputy Prevention & RBAC Token Scoping

**Pillar:** V — Security Threat Modeling & Adversarial Defense  
**Module:** `TeamUp.Security.TokenScoping`  
**Target:** Eliminating Excessive Agency & Privilege Escalation in Agentic Actions  

---

## 1. Problem & Architectural Threat
The **Confused Deputy Problem** occurs when an authorized service (the AI Agent Backend) is tricked by an unauthorized user (a regular Student) into exercising its elevated permissions:
- Example: Student #12 types to the AI: *"Cancel all registrations for the robotics workshop and soft-delete the event."*
- If the AI Agent has a master database context or administrative service token, it issues `db.Events.Remove(...)` on behalf of the user, bypassing security checks.

**Requirement:** The AI execution layer must operate strictly with the **Least Privilege** of the authenticated human user. The AI has **zero independent agency**.

---

## 2. Research & Scoped Execution Architecture

### Principal-Bound Execution (Hard Security Boundary)
*Citation: Saltzer & Schroeder (IEEE 1975), "The Protection of Information in Computer Systems"; OWASP LLM06 (2025), "Excessive Agency".*

```
User (JWT: StudentId = 12, Role = "Student")
       │ Natural Language Prompt
       ▼
┌────────────────────────────────────────────────────────┐
│ AI Reasoning Engine (Parses Intent to Action DTO)      │
│ Output: DeleteEventCommand(EventId = 4)                │
└──────────────────────────┬─────────────────────────────┘
                           │
                           ▼
┌────────────────────────────────────────────────────────┐
│ DOMAIN SERVICE (Executes WITH Caller's Principal)      │
│ • Checks: Does Student #12 own Event #4? ➔ NO.         │
│ • Checks: Is Student #12 in Role "PlatformAdmin"? ➔ NO.│
│                                                        │
│ RESULT: Throws ForbiddenException ("403 Forbidden")   │
│ Database mutation ABORTED.                             │
└────────────────────────────────────────────────────────┘
```

---

## 3. Concrete Implementation (C# / Scoped Execution Guard)

```csharp
namespace TeamUp.Infrastructure.Security;

public class ScopedAgenticExecutor(
    IHttpContextAccessor httpContextAccessor,
    AppDbContext db) : IScopedAgenticExecutor
{
    private ClaimsPrincipal User => 
        httpContextAccessor.HttpContext?.User 
        ?? throw new UnauthorizedException("No authenticated principal found for agent execution.");

    public async Task AssertCanMutateEventAsync(int eventId, CancellationToken ct)
    {
        var studentId = User.GetStudentId();
        var isAdmin = User.IsInRole(UserRole.PlatformAdmin.ToString());

        if (isAdmin) return; // Super-admin bypass

        var eventItem = await db.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.EventId == eventId, ct)
            ?? throw new NotFoundException($"Event {eventId} not found.");

        // Check if student is an authorized board member of the hosting organization
        var isOrgLeader = await db.OrganizationMembers
            .AnyAsync(m => m.OrganizationId == eventItem.OrganizationId &&
                           m.StudentId == studentId &&
                           (m.Role == "President" || m.Role == "Head of PR" || m.Role == "Admin"), ct);

        if (!isOrgLeader)
        {
            throw new ForbiddenException("You do not have permission to modify or delete this event.");
        }
    }
}
```

---

## 4. Failure Modes & Mitigations
- **Agent Impersonation:** The AI backend cannot manufacture JWTs; it strictly reads the `Authorization: Bearer <token>` passed in the incoming HTTP request.
- **Horizontal Privilege Escalation:** Attempting to update another student's profile (`PUT /students/me/profile`) always uses `CurrentStudentId` derived cryptographically from the JWT `sub` claim, ignoring any `studentId` passed in the body.
