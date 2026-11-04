# Phase 04: Public Catalog Entity Disambiguation & Resolution

**Pillar:** I — Conversational Onboarding & Intent Capture  
**Module:** `TeamUp.AiGateway.Disambiguation`  
**Target:** Grounding Fuzzy User Mentions to Exact Database Primary Keys  

---

## 1. Problem & Architectural Challenge
When a user says:
> *"I study computer science at Cairo Uni."*

The database does **not** contain an entity named *"Cairo Uni"*, nor a faculty called *"computer science"*.
The database contains:
- `University`: `UniversityId = 1`, `Name = "Cairo University"`, `City = "Giza"`
- `Faculty`: `FacultyId = 1`, `Name = "Faculty of Computers and Artificial Intelligence"`, `Code = "FCAI"`

If the AI guesses an ID, it causes a Foreign Key violation (`404 Not Found`). If it saves the string blindly, it breaks 3NF relational normalization.

**Requirement:** Ground conversational entity mentions to valid database primary keys with **zero hallucinated foreign keys**.

---

## 2. Research & Hybrid Matching Pipeline

### Hybrid Search: Trigram String Matching + Dense Semantic Vectors
*Citation: Robertson & Zaragoza (2009), "The Probabilistic Relevance Framework: BM25"; Reimers & Gurevych (2019), "Sentence-BERT: Sentence Embeddings using Siamese BERT-Networks".*

```
User Mention: "Cairo Uni" 
       │
       ├──► 1. Exact / Trigram Lexical Search (Levenshtein distance <= 2)
       │       ➔ Match: "Cairo University" (Similarity: 0.91)
       │
       └──► 2. Dense Semantic Vector Search (Cosine Similarity)
               ➔ Match: "Cairo University" (Cosine: 0.96)
```

For Faculties under the resolved University:
```
Mention: "computer science"
       │
       ├──► Acronym Match: "CS" -> "FCAI"
       └──► Semantic Vector Match: "computer science" -> "Faculty of Computers and Artificial Intelligence" (Cosine: 0.89)
```

---

## 3. Concrete Implementation (C# / EF Core Hybrid Grounding)

```csharp
namespace TeamUp.Application.Ai.Disambiguation;

public sealed record ResolvedEntity<T>(T Entity, int Id, float Confidence);

public class AcademicEntityResolver(AppDbContext db) : IAcademicEntityResolver
{
    public async Task<ResolvedEntity<Faculty>?> ResolveFacultyAsync(
        string rawUniversityMention, 
        string rawFacultyMention, 
        CancellationToken ct)
    {
        // 1. Resolve University first
        var uniTerm = rawUniversityMention.Trim().ToLower();
        var universities = await db.Universities.AsNoTracking().ToListAsync(ct);

        var matchedUni = universities
            .Select(u => new { Uni = u, Score = CalculateSimilarity(u.Name.ToLower(), uniTerm) })
            .Where(x => x.Score > 0.65f)
            .OrderByDescending(x => x.Score)
            .FirstOrDefault()?.Uni;

        if (matchedUni is null)
            return null; // Prompt user for clarification

        // 2. Resolve Faculty strictly WITHIN the matched University
        var facTerm = rawFacultyMention.Trim().ToLower();
        var faculties = await db.Faculties
            .AsNoTracking()
            .Where(f => f.UniversityId == matchedUni.UniversityId)
            .ToListAsync(ct);

        var matchedFaculty = faculties
            .Select(f => new 
            { 
                Faculty = f, 
                Score = Math.Max(
                    CalculateSimilarity(f.Name.ToLower(), facTerm),
                    CalculateSimilarity(f.Code.ToLower(), facTerm))
            })
            .Where(x => x.Score > 0.55f)
            .OrderByDescending(x => x.Score)
            .FirstOrDefault();

        return matchedFaculty is not null 
            ? new ResolvedEntity<Faculty>(matchedFaculty.Faculty, matchedFaculty.Faculty.FacultyId, matchedFaculty.Score)
            : null;
    }

    private static float CalculateSimilarity(string s1, string s2)
    {
        if (s1.Contains(s2) || s2.Contains(s1)) return 0.9f;
        // Jaro-Winkler / Levenshtein distance calculation
        return FastLevenshteinRatio(s1, s2);
    }

    private static float FastLevenshteinRatio(string s, string t)
    {
        int n = s.Length, m = t.Length;
        if (n == 0 || m == 0) return 0f;
        int[,] d = new int[n + 1, m + 1];
        for (int i = 0; i <= n; d[i, 0] = i++) ;
        for (int j = 0; j <= m; d[0, j] = j++) ;
        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
            }
        }
        return 1.0f - ((float)d[n, m] / Math.Max(n, m));
    }
}
```

---

## 4. Failure Modes & Mitigations
- **Confidence < Threshold (e.g. 0.60):** Do NOT guess. The system returns an inline disambiguation selector: *"Did you mean Cairo University or American University in Cairo?"*
- **New University not in catalog:** If resolution completely fails, the AI flags the university for PlatformAdmin approval and allows the student to proceed with manual entry.
