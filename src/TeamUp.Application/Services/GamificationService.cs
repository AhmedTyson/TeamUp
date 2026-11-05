using Microsoft.EntityFrameworkCore;
using TeamUp.Application.Common;
using TeamUp.Application.Common.Exceptions;
using TeamUp.Application.Common.Interfaces;
using TeamUp.Application.Gamification;
using TeamUp.Domain.Entities;
using TeamUp.Domain.Enums;

namespace TeamUp.Application.Services;

public sealed class GamificationService(IAppDbContext db) : IGamificationService
{
    public StudentScoreDto CalculateLevelMetrics(int studentId, int totalPoints)
    {
        var effectivePoints = Math.Max(totalPoints, 0);
        var level = (int)Math.Floor(Math.Sqrt(effectivePoints / 100.0)) + 1;
        var currentLevelStart = (int)Math.Pow(level - 1, 2) * 100;
        var nextLevelThreshold = (int)Math.Pow(level, 2) * 100;
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
            CreatedAt = DateTime.UtcNow,
        };

        db.ScoreTransactions.Add(tx);
        await db.SaveChangesAsync(ct);

        return new ScoreTransactionDto(
            tx.TransactionId,
            tx.StudentId,
            tx.Points,
            tx.Reason,
            tx.SourceType,
            tx.CreatedAt);
    }
}
