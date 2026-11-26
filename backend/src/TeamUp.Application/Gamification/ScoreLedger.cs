// Date: 2026-11-24
// Purpose: Idempotent XP awarding. The database unique index on (StudentId, SourceType, SourceReferenceId) is the source of truth;
// the pre-check only avoids a needless failed insert.

using Microsoft.EntityFrameworkCore;
using TeamUp.Application.Common.Interfaces;
using TeamUp.Domain.Entities;
using TeamUp.Domain.Enums;

namespace TeamUp.Application.Gamification;

public static class ScoreLedger
{
    /// <summary>Awards points once per (student, source, reference). Returns false when the award already exists.</summary>
    public static async Task<bool> TryAwardAsync(
        IAppDbContext db,
        int studentId,
        int points,
        string reason,
        ScoreSourceType sourceType,
        int sourceReferenceId,
        CancellationToken ct,
        TimeProvider? timeProvider = null)
    {
        if (reason.Length > 200)
            reason = reason[..200];

        var exists = () => db.ScoreTransactions.AsNoTracking().AnyAsync(
            t => t.StudentId == studentId && t.SourceType == sourceType && t.SourceReferenceId == sourceReferenceId, ct);

        if (await exists())
            return false;

        var tx = new ScoreTransaction
        {
            StudentId = studentId,
            Points = points,
            Reason = reason,
            SourceType = sourceType,
            SourceReferenceId = sourceReferenceId,
            CreatedAt = (timeProvider ?? TimeProvider.System).GetUtcNow().UtcDateTime
        };
        db.ScoreTransactions.Add(tx);

        try
        {
            await db.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateException)
        {
            // A concurrent request may have inserted the same award between the check and the insert: benign duplicate.
            db.Entry(tx).State = EntityState.Detached;
            if (await exists())
                return false;
            throw;
        }
    }
}
