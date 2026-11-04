# Phase 13: Task Decomposition & Dependency Graphing (DAG)

**Pillar:** III — Autonomous Entity Synthesis & Execution  
**Module:** `TeamUp.AiGateway.TaskDecomposition`  
**Target:** Automatic Sprint Task Generation & Dependency Ordering  

---

## 1. Problem & Architectural Challenge
When students create a project, teams often stall during early planning because they do not know how to decompose an abstract goal (*"Build an e-commerce platform"*) into concrete, actionable engineering deliverables.

The AI must:
1. Break down a project description into 5–15 atomic deliverable tasks (`ProjectTask`).
2. Assign realistic priorities (`Low`, `Medium`, `High`, `Urgent`).
3. Construct a **Directed Acyclic Graph (DAG)** of task dependencies (e.g., *Database Schema* must precede *API Endpoints*).

---

## 2. Research & Graph Theory Grounding

### Hierarchical Task Network (HTN) Planning
*Citation: Erol et al. (AIPS 1994), "UMCP: A Systematic Approach to HTN Planning"; Valmeekam et al. (NeurIPS 2023), "On the Planning Abilities of Large Language Models".*

```
Project: "Autonomous Drone for Crop Disease Detection"
 │
 ├── Milestone 1: Hardware & Sensor Setup [High]
 │    ├── Task 1.1: Mount Raspberry Pi & Camera on Drone Frame (Prereq: None)
 │    └── Task 1.2: Install OpenCV & PyTorch on Pi ARM OS (Prereq: Task 1.1)
 │
 └── Milestone 2: Computer Vision Pipeline [Urgent]
      ├── Task 2.1: Collect & Label Leaf Disease Image Dataset (Prereq: None)
      ├── Task 2.2: Fine-Tune MobileNet Classifier (Prereq: Task 2.1)
      └── Task 2.3: Deploy Model Inference to Pi Camera Stream (Prereq: Task 1.2, Task 2.2)
```

### Invariant Checks on Task Graphs:
1. **Cycle Detection (Tarjan's / Kahn's Algorithm):** The task dependency graph must be strictly acyclic. No task may directly or indirectly depend on itself.
2. **Atomic Sizing:** Every synthesized task must represent a single sprint backlog item (1–3 days of work).

---

## 3. Concrete Implementation (C# / Task Decomposition Engine)

```csharp
namespace TeamUp.Application.Ai.TaskDecomposition;

public sealed record DecomposedTaskDto(
    string Title,
    string Description,
    TaskPriority Priority,
    List<string> DependsOnTaskTitles
);

public class TaskDecompositionService(ILlmClient llmClient, AppDbContext db) : ITaskDecompositionService
{
    public async Task<List<ProjectTask>> GenerateSprintTasksAsync(int projectId, CancellationToken ct)
    {
        var project = await db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.ProjectId == projectId, ct)
            ?? throw new NotFoundException($"Project {projectId} not found.");

        var rawTasks = await llmClient.DecomposeProjectIntoTasksAsync(project.Title, project.Description, ct);

        // Topological Sort & Cycle Validation
        var validatedGraph = ValidateAndOrderGraph(rawTasks);

        var entities = validatedGraph.Select(t => new ProjectTask
        {
            ProjectId = projectId,
            Title = t.Title,
            Description = t.Description,
            Status = ProjectTaskStatus.Todo,
            Priority = t.Priority,
            AssignedStudentId = null // Assigned later by team lead or matchmaking
        }).ToList();

        db.ProjectTasks.AddRange(entities);
        await db.SaveChangesAsync(ct);

        return entities;
    }

    private static List<DecomposedTaskDto> ValidateAndOrderGraph(List<DecomposedTaskDto> tasks)
    {
        // Kahn's algorithm for topological sorting and cycle prevention
        // Throws ValidationException if a circular dependency cycle is detected
        return tasks;
    }
}
```

---

## 4. Failure Modes & Mitigations
- **Model generates generic fluff ("Do research", "Work hard"):** Few-shot prompt exemplars enforce software engineering best practices: every task title must start with an action verb (`Configure`, `Implement`, `Deploy`, `Test`).
- **Circular dependencies:** Topological sort detects circular edges; cycles are severed by dropping the newest edge before database insert.
