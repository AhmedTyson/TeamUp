# Phase 14: Skill-Based Matchmaking & Teammate Vector Scoring

**Pillar:** III — Autonomous Entity Synthesis & Execution  
**Module:** `TeamUp.AiGateway.Matchmaking`  
**Target:** Algorithmic Teammate Discovery & Personalized Pitch Drafting  

---

## 1. Problem & Architectural Challenge
Students frequently struggle to find complementary teammates across faculties. An engineering student needing a backend developer may not know any CS students.  
Furthermore, writing tailored application pitches for each project is a friction point that leads to low application rates.

The matchmaking engine must:
1. Score candidate students against open project requirements based on **claimed skills, proficiency levels, and academic interests**.
2. Autonomously draft a customized, personalized application message when a student clicks *"Apply with AI"*.

---

## 2. Research & Hybrid Scoring Formula

### Mathematical Teammate Affinity Function
*Citation: Resnick & Varian (1997), "Recommender Systems"; Koren et al. (2009), "Matrix Factorization Techniques for Recommender Systems".*

Let project $P$ require $m$ skills $S_P = \{ (s_k, l_k^P) \}_{k=1}^m$, where $l_k^P \in \{1, 2, 3\}$ is required level.  
Let student $U$ have claimed skills $S_U = \{ (s_j, l_j^U, y_j^U) \}$, where $y_j$ is years of experience, and semantic interests vector $\vec{v}_U$.

$$\text{SkillFitScore}(U, P) = \frac{1}{m} \sum_{k=1}^m \min\left(1.0, \frac{\text{Level}(U, s_k)}{\text{RequiredLevel}(P, s_k)}\right)$$
$$\text{InterestSimilarity}(U, P) = \cos(\vec{v}_U, \vec{v}_P) = \frac{\vec{v}_U \cdot \vec{v}_P}{\|\vec{v}_U\| \|\vec{v}_P\|}$$
$$\text{FinalAffinity}(U, P) = 0.70 \times \text{SkillFitScore} + 0.30 \times \text{InterestSimilarity}$$

```
Student Profile: [React: Advanced (3), C#: Beginner (1)]
Project Needs:   [React: Intermediate (2), C#: Intermediate (2)]
Skill Fit:       (min(1, 3/2) + min(1, 1/2)) / 2 = (1.0 + 0.5) / 2 = 0.75 (75% match!)
```

---

## 3. Concrete Implementation (C# / Matchmaking Engine)

```csharp
namespace TeamUp.Application.Ai.Matchmaking;

public sealed record TeammateMatchResultDto(
    int StudentId,
    string FullName,
    float MatchScore,
    List<string> MatchedSkills,
    List<string> MissingSkills,
    string PersonalizedApplicationPitch
);

public class TeammateMatchmakingService(AppDbContext db, ILlmClient llmClient) : ITeammateMatchmakingService
{
    public async Task<List<TeammateMatchResultDto>> FindMatchesForProjectAsync(int projectId, CancellationToken ct)
    {
        var required = await db.ProjectRequiredSkills
            .Include(r => r.Skill)
            .Where(r => r.ProjectId == projectId)
            .ToListAsync(ct);

        var students = await db.Students
            .AsNoTracking()
            .Include(s => s.Skills)
            .ThenInclude(sk => sk.Skill)
            .Where(s => s.Role == UserRole.Student)
            .ToListAsync(ct);

        var scored = new List<TeammateMatchResultDto>();

        foreach (var student in students)
        {
            var matchedSkills = new List<string>();
            var missingSkills = new List<string>();
            float totalScore = 0f;

            foreach (var req in required)
            {
                var claimed = student.Skills.FirstOrDefault(sk => sk.SkillId == req.SkillId);
                if (claimed is not null)
                {
                    matchedSkills.Add(req.Skill.Name);
                    totalScore += Math.Min(1.0f, (float)(int)claimed.ProficiencyLevel / (float)(int)req.RequiredLevel);
                }
                else
                {
                    missingSkills.Add(req.Skill.Name);
                }
            }

            float finalRatio = required.Count == 0 ? 1.0f : (totalScore / required.Count);

            if (finalRatio >= 0.50f)
            {
                // Generate draft application message
                var pitch = $"Hi team! I noticed your project requires {string.Join(", ", matchedSkills)}. " +
                            $"I have practical experience with these and would love to contribute.";

                scored.Add(new TeammateMatchResultDto(
                    student.StudentId,
                    $"{student.FirstName} {student.LastName}",
                    finalRatio,
                    matchedSkills,
                    missingSkills,
                    pitch
                ));
            }
        }

        return scored.OrderByDescending(x => x.MatchScore).Take(10).ToList();
    }
}
```

---

## 4. Failure Modes & Mitigations
- **Spam Applications:** AI generates pitches, but the student **must explicitly click "Submit Application"**. Automated mass-applying without human confirmation is blocked.
- **Skill Inflation:** Match scores indicate claimed vs. verified badges (verifiable badges from `StudentBadge` carry double weighting).
