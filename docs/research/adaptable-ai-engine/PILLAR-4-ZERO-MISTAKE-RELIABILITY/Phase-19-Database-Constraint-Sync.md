# Phase 19: Database Filtered Constraint Synchronization

**Pillar:** IV — The Zero-Mistake Deterministic Reliability Shield  
**Module:** `TeamUp.AiGateway.ConstraintSync`  
**Target:** Automated Synchronization of Relational Schema Constraints to LLM Context  

---

## 1. Problem & Architectural Challenge
Database constraints evolve:
- An academic year check constraint is modified from `[1..5]` to `[1..7]`.
- An event capacity check constraint is set to `[Capacity] >= 1`.
- A filtered unique index is created: `CREATE UNIQUE INDEX [IX_Student_Email] ON Student(Email) WHERE [DeletedAt] IS NULL`.

If system prompts and AI JSON schemas are maintained by hand in separate files:
1. **Schema Drift:** Developers update `AppDbContext.cs`, but forget to update the AI prompt.
2. The AI generates data that passed prompt validation, but crashes at the SQL database layer.

**Requirement:** Automatically derive AI validation schemas directly from EF Core entity metadata at runtime/compile-time (**Single Source of Truth**).

---

## 2. Research & Metadata Reflection Architecture

### Automated Derivation from EF Core `IModel`
*Citation: Fowler (2006), "Domain-Specific Languages"; Microsoft EF Core Documentation, "Model Metadata APIs".*

```
EF Core AppDbContext (C# Entity Configurations)
       │
       ▼ Model Reflection Engine
 Reads IEntityType, IProperty, ICheckConstraint, IIndex
       │
       ├──► Extracts MaxLength, IsRequired, Nullability
       ├──► Extracts CHECK constraints (Regex & Range predicates)
       ├──► Extracts Filtered Unique Indexes (WHERE [DeletedAt] IS NULL)
       │
       ▼ Automated Export
 Target JSON Schema with Embedded Pydantic / System.Text.Json Constraints
```

---

## 3. Concrete Implementation (C# / Schema Metadata Reflector)

```csharp
namespace TeamUp.Infrastructure.Ai.Metadata;

public class EfModelConstraintExtractor(AppDbContext db) : IEfModelConstraintExtractor
{
    public Dictionary<string, object> ExtractConstraintsForEntity<TEntity>() where TEntity : class
    {
        var entityType = db.Model.FindEntityType(typeof(TEntity))
            ?? throw new InvalidOperationException($"Entity {typeof(TEntity).Name} not registered in DbContext.");

        var constraints = new Dictionary<string, object>();

        foreach (var property in entityType.GetProperties())
        {
            var propInfo = new Dictionary<string, object>
            {
                ["type"] = property.ClrType.Name,
                ["isNullable"] = property.IsNullable
            };

            var maxLength = property.GetMaxLength();
            if (maxLength.HasValue) propInfo["maxLength"] = maxLength.Value;

            constraints[property.Name] = propInfo;
        }

        // Extract SQL Server CHECK constraints
        var checkConstraints = entityType.GetCheckConstraints()
            .Select(c => new { c.Name, c.Sql })
            .ToList();
        constraints["_checkConstraints"] = checkConstraints;

        // Extract Filtered Unique Indexes
        var filteredIndexes = entityType.GetIndexes()
            .Where(idx => idx.IsUnique && !string.IsNullOrEmpty(idx.GetFilter()))
            .Select(idx => new 
            { 
                Columns = idx.Properties.Select(p => p.Name).ToList(),
                Filter = idx.GetFilter() 
            })
            .ToList();
        constraints["_filteredUniqueIndexes"] = filteredIndexes;

        return constraints;
    }
}
```

---

## 4. Failure Modes & Mitigations
- **Complex SQL CHECK Expressions (e.g. `[AcademicYear] BETWEEN 1 AND 7`):** Regex parser extracts numeric bounds and injects `"minimum": 1, "maximum": 7` directly into the AI's grammar-constrained JSON schema.
- **CI Schema Drift Gate:** A GitHub Actions test verifies that every entity schema injected into AI prompts matches the live SQL Server migration snapshot with 0 discrepancies.
