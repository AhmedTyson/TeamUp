# Phase 26: Asynchronous Processing & SignalR Streaming Pipeline

**Pillar:** VI — Production Architecture, Observability & Governance  
**Module:** `TeamUp.Infrastructure.AsyncWorkers`  
**Target:** Non-Blocking Multimodal Processing & Real-Time Client Feedback  

---

## 1. Problem & Architectural Challenge
Multimodal vision processing (OCR + ViT classification + LLM entity synthesis) can take **3 to 7 seconds**:
- Holding an HTTP connection open synchronously risks gateway proxy timeouts (HTTP 504) on mobile connections.
- Users staring at a static loading spinner for 5 seconds assume the website is frozen and reload, triggering duplicate expensive workloads.

**Requirement:** Asynchronous background job offloading (Hangfire / RabbitMQ / Redis Streams) with real-time, step-by-step progress streaming to the browser via **ASP.NET Core SignalR**.

---

## 2. Research & Streaming Event Architecture

```
User drops Flyer (POST /api/v1/ai/jobs/flyer-synthesis)
       │
       ▼ Returns HTTP 202 Accepted { jobId: "job_8f12a..." }
       │
[Background Worker Task]
       ├──► Step 1 (15%): Normalizing image & virus scan
       │       └── SignalR Broadcast: { step: "Scanning", progress: 15 }
       │
       ├──► Step 2 (45%): Vision model parsing visual timetable
       │       └── SignalR Broadcast: { step: "Extracting Sessions", progress: 45 }
       │
       ├──► Step 3 (75%): Grounding university & organization catalog
       │       └── SignalR Broadcast: { step: "Resolving Entities", progress: 75 }
       │
       └──► Step 4 (100%): Synthesizing complete EventDraftCard
               └── SignalR Broadcast: { step: "Complete", progress: 100, payload: {...} }
```

---

## 3. Concrete Implementation (C# / SignalR Hub & Background Worker)

### SignalR Onboarding Hub
```csharp
namespace TeamUp.Api.Hubs;

public class OnboardingHub : Hub
{
    public async Task JoinSessionGroup(string sessionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"session_{sessionId}");
    }
}
```

### Background Progress Dispatcher
```csharp
namespace TeamUp.Infrastructure.AsyncWorkers;

public class AsyncSynthesisWorker(
    IHubContext<OnboardingHub> hubContext,
    IFlyerExtractionService flyerService,
    IAutonomousActionCoordinator coordinator) : IAsyncSynthesisWorker
{
    public async Task ProcessFlyerJobAsync(string sessionId, string blobUrl, CancellationToken ct)
    {
        await NotifyAsync(sessionId, 20, "Analyzing visual layout...");

        var draft = await flyerService.ExtractFromBlobUrlAsync(blobUrl, ct);

        await NotifyAsync(sessionId, 70, "Grounding academic catalog and schedules...");

        // Final payload delivery
        await hubContext.Clients.Group($"session_{sessionId}")
            .SendAsync("OnJobCompleted", new { success = true, draft }, ct);
    }

    private async Task NotifyAsync(string sessionId, int percent, string statusMessage)
    {
        await hubContext.Clients.Group($"session_{sessionId}")
            .SendAsync("OnProgressUpdate", new { progress = percent, message = statusMessage });
    }
}
```

---

## 4. Failure Modes & Mitigations
- **WebSocket Disconnection:** If the browser loses WebSocket connection, it falls back to HTTP long-polling or polls `GET /api/v1/ai/jobs/{jobId}` every 2 seconds.
- **Worker Crash:** Hangfire / RabbitMQ persistent queues ensure unacknowledged jobs are retried on healthy worker nodes.
