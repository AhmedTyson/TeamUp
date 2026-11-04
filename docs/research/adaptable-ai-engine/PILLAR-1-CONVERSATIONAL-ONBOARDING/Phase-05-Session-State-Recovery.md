# Phase 05: Session State Recovery & Ephemeral-to-Auth Migration

**Pillar:** I — Conversational Onboarding & Intent Capture  
**Module:** `TeamUp.AiGateway.SessionManagement`  
**Target:** Safe Migration from Anonymous Conversation to Authenticated Account  

---

## 1. Problem & Architectural Challenge
When a guest interacts with the hero prompt on the landing page, they are **unauthenticated**.
They may spend 3 minutes chatting, specifying their interests, and dropping an avatar before setting a password.

If the session crashes, browser reloads, or auth fails:
1. The user loses their extracted profile data.
2. The system risks creating orphaned database rows for unverified accounts.

**Requirement:** Maintain an ephemeral, encrypted staging state in distributed cache (Redis) that seamlessly hydrates into a persistent database account upon verified credential creation.

---

## 2. Research & State Flow Architecture

### Ephemeral-to-Permanent State Transition Flow
```
Guest User lands on /
       │ Types prompt
       ▼
[Client] Generates Cryptographic Session Secret (AES-GCM client token)
       │
       ▼
[Redis Cache] Stores Staging State (TTL = 45 Minutes):
       {
         "sessionId": "anon_9f81a...",
         "extractedSlots": { "firstName": "Ahmed", "lastName": "Tyson", ... },
         "stagedAvatarUrl": "blob://temp/anon_9f81a.png",
         "stagedInterests": ["AI", "Robotics"]
       }
       │
       ▼ User confirms profile & supplies password: "StrongP@ssw0rd2026!"
       │
[Atomic Transaction Commit (AppDbContext)]:
       1. Check Email uniqueness WHERE [DeletedAt] IS NULL
       2. Insert Student (PasswordHash, FacultyId, AcademicYear)
       3. Insert StudentProfile (PhotoUrl = stagedAvatarUrl)
       4. Insert StudentInterests (stagedInterests)
       5. Delete Redis Key "anon_9f81a..."
       6. Return JWT Access + Refresh Token
```

---

## 3. Data Contracts & Concrete Implementation

### Staging Cache Manager (C# / StackExchange.Redis / IDistributedCache)
```csharp
namespace TeamUp.Application.Ai.SessionManagement;

public sealed record StagedOnboardingState(
    string SessionId,
    string FirstName,
    string LastName,
    string Email,
    int AcademicYear,
    int FacultyId,
    string? StagedAvatarBlobUrl,
    List<string> Interests,
    DateTime CreatedAt
);

public class OnboardingSessionManager(IDistributedCache cache) : IOnboardingSessionManager
{
    private static string Key(string sessionId) => $"onboarding_session:{sessionId}";

    public async Task SaveDraftAsync(StagedOnboardingState state, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(state);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(45)
        };
        await cache.SetStringAsync(Key(state.SessionId), json, options, ct);
    }

    public async Task<StagedOnboardingState?> GetDraftAsync(string sessionId, CancellationToken ct)
    {
        var json = await cache.GetStringAsync(Key(sessionId), ct);
        return string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<StagedOnboardingState>(json);
    }

    public async Task ClearDraftAsync(string sessionId, CancellationToken ct)
    {
        await cache.RemoveAsync(Key(sessionId), ct);
    }
}
```

---

## 4. Failure Modes & Mitigations
- **Abandoned Sessions (Zombie Drafts):** Redis TTL automatically evicts expired sessions after 45 minutes; background blob garbage collector purges unreferenced avatar uploads.
- **Race Condition on Final Registration:** If two tabs submit the same staging token, the first wins via atomic database unique constraint; the second receives an idempotent `200 OK` or `409 Conflict`.
