// Date: 2026-11-21
// Purpose: Append-only gamification scoring ledger service with level math, ranked leaderboard, and manual-adjustment audit logging.

using Microsoft.EntityFrameworkCore;
using TeamUp.Application.Common;
using TeamUp.Application.Common.Exceptions;
using TeamUp.Application.Common.Interfaces;
using TeamUp.Application.Common.Security;
using TeamUp.Application.Gamification;
using TeamUp.Domain.Entities;
using TeamUp.Domain.Enums;

namespace TeamUp.Application.Services;

public sealed class GamificationService(
    IAppDbContext db,
    IAuditLogService? auditLog = null,
    Microsoft.Extensions.Options.IOptions<GamificationOptions>? gamificationOptions = null,
    TimeProvider? timeProvider = null) : IGamificationService
{
    private readonly TimeProvider _time = timeProvider ?? TimeProvider.System;
    #region Level Calculation & Score Ledger

    public StudentScoreDto CalculateLevelMetrics(int studentId, int totalPoints)
    {
        // LevelStepPoints is configurable; a non-positive configured value falls back to the default curve.
        var step = gamificationOptions?.Value.LevelStepPoints > 0
            ? gamificationOptions.Value.LevelStepPoints
            : new GamificationOptions().LevelStepPoints;
        var effectivePoints = Math.Max(totalPoints, 0);
        var level = (int)Math.Floor(Math.Sqrt(effectivePoints / (double)step)) + 1;
        var currentLevelStart = (int)Math.Pow(level - 1, 2) * step;
        var nextLevelThreshold = (int)Math.Pow(level, 2) * step;
        var pointsToNext = Math.Max(nextLevelThreshold - totalPoints, 0);

        double progress = 0.0;
        var span = nextLevelThreshold - currentLevelStart;
        if (span > 0)
        {
            var rawProgress = ((double)(effectivePoints - currentLevelStart) / span) * 100.0;
            progress = Math.Max(0.0, Math.Min(100.0, rawProgress));
        }

        return new StudentScoreDto(
            studentId,
            totalPoints,
            level,
            currentLevelStart,
            nextLevelThreshold,
            pointsToNext,
            Math.Round(progress, 2));
    }

    public async Task<StudentScoreDto> GetStudentScoreAsync(int studentId, CancellationToken ct = default)
    {
        var studentExists = await db.Students.AnyAsync(s => s.StudentId == studentId, ct);
        if (!studentExists)
            throw new NotFoundException($"Student with ID {studentId} was not found.");

        var totalPoints = await db.ScoreTransactions
            .Where(t => t.StudentId == studentId)
            .SumAsync(t => (int?)t.Points, ct) ?? 0;

        return CalculateLevelMetrics(studentId, totalPoints);
    }

    public async Task<PagedResult<ScoreTransactionDto>> GetScoreTransactionsAsync(int studentId, PageRequest page, CancellationToken ct = default)
    {
        var studentExists = await db.Students.AnyAsync(s => s.StudentId == studentId, ct);
        if (!studentExists)
            throw new NotFoundException($"Student with ID {studentId} was not found.");

        var query = db.ScoreTransactions.AsNoTracking().Where(t => t.StudentId == studentId);
        var total = await query.CountAsync(ct);

        var items = await query.OrderByDescending(t => t.CreatedAt)
            .Skip(page.Skip).Take(page.SafePageSize)
            .Select(t => new ScoreTransactionDto(
                t.TransactionId,
                t.StudentId,
                t.Points,
                t.Reason,
                t.SourceType,
                t.CreatedAt))
            .ToListAsync(ct);

        return new PagedResult<ScoreTransactionDto>(items, page.SafePage, page.SafePageSize, total);
    }

    public async Task<ScoreTransactionDto> AddScoreTransactionAsync(
        int studentId, int points, string reason, ScoreSourceType sourceType = ScoreSourceType.Manual, CancellationToken ct = default)
    {
        var studentExists = await db.Students.AnyAsync(s => s.StudentId == studentId, ct);
        if (!studentExists)
            throw new NotFoundException($"Student with ID {studentId} was not found.");

        var tx = new ScoreTransaction
        {
            StudentId = studentId,
            Points = points,
            Reason = reason.Trim(),
            SourceType = sourceType,
            CreatedAt = _time.GetUtcNow().UtcDateTime,
        };

        db.ScoreTransactions.Add(tx);
        await db.SaveChangesAsync(ct);

        if (sourceType == ScoreSourceType.Manual && auditLog is not null)
        {
            await auditLog.RecordAsync(
                "ManualScoreAdjusted",
                "ScoreTransaction",
                tx.TransactionId.ToString(),
                newValues: new { studentId, points, tx.Reason, SourceType = sourceType.ToString() },
                ct: ct);
        }

        return new ScoreTransactionDto(
            tx.TransactionId,
            tx.StudentId,
            tx.Points,
            tx.Reason,
            tx.SourceType,
            tx.CreatedAt);
    }

    #endregion

    #region Campus Leaderboard

    public async Task<PagedResult<LeaderboardEntryDto>> GetLeaderboardAsync(PageRequest page, CancellationToken ct = default)
    {
        var safePageSize = Math.Clamp(page.PageSize, 1, 50);
        var safePage = Math.Max(1, page.Page);
        var skip = (safePage - 1) * safePageSize;

        var total = await db.Students.AsNoTracking().CountAsync(ct);

        var scored = await db.Students
            .AsNoTracking()
            .Select(s => new
            {
                s.StudentId,
                s.FirstName,
                s.LastName,
                s.AffiliationType,
                s.City,
                IsEmailVerified = s.EmailVerifiedAtUtc != null,
                FacultyName = s.Faculty != null ? s.Faculty.Name : null,
                UniversityName = s.Faculty != null ? s.Faculty.University.Name : null,
                PhotoUrl = s.Profile != null ? s.Profile.PhotoUrl : null,
                TotalPoints = db.ScoreTransactions
                    .Where(t => t.StudentId == s.StudentId)
                    .Sum(t => (int?)t.Points) ?? 0
            })
            .OrderByDescending(x => x.TotalPoints)
            .ThenBy(x => x.StudentId)
            .Skip(skip)
            .Take(safePageSize)
            .ToListAsync(ct);

        var entries = scored
            .Select((row, index) =>
            {
                var metrics = CalculateLevelMetrics(row.StudentId, row.TotalPoints);
                return new LeaderboardEntryDto(
                    Rank: skip + index + 1,
                    row.StudentId,
                    row.FirstName,
                    row.LastName,
                    row.FacultyName ?? AffiliationFormatting.FormatAffiliationLabel(row.AffiliationType),
                    row.UniversityName ?? (string.IsNullOrWhiteSpace(row.City) ? "Egypt Open Community" : $"{row.City} Community"),
                    row.TotalPoints,
                    metrics.Level,
                    row.PhotoUrl,
                    row.IsEmailVerified);
            })
            .ToList();

        return new PagedResult<LeaderboardEntryDto>(entries, safePage, safePageSize, total);
    }

    #endregion
}
