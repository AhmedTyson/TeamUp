using Microsoft.EntityFrameworkCore;
using TeamUp.Api.Data;
using TeamUp.Api.Data.Entities;
using TeamUp.Api.Models.DTOs;

namespace TeamUp.Api.Services;

public interface IGamificationService
{
    StudentScoreResponseDto CalculateLevelMetrics(string studentId, int totalPoints);
    Task<StudentScoreResponseDto> GetStudentScoreAsync(string studentId);
    Task<ScoreTransactionListResponseDto> GetScoreTransactionsAsync(string studentId, string? sourceType, int page, int limit);
    Task<ScoreTransactionResponseDto> AddScoreTransactionAsync(string studentId, int points, string reason, string sourceType = "ManualAdjustment", string? sourceId = null);
}

public class GamificationService : IGamificationService
{
    private readonly TeamUpDbContext _db;
    private readonly IBadgeService _badgeService;
    private readonly INotificationService _notificationService;

    public GamificationService(
        TeamUpDbContext db,
        IBadgeService badgeService,
        INotificationService notificationService)
    {
        _db = db;
        _badgeService = badgeService;
        _notificationService = notificationService;
    }

    public StudentScoreResponseDto CalculateLevelMetrics(string studentId, int totalPoints)
    {
        var effectivePoints = Math.Max(totalPoints, 0);
        var level = (int)Math.Floor(Math.Sqrt(effectivePoints / 100.0)) + 1;
        var currentLevelStart = (int)Math.Pow(level - 1, 2) * 100;
        var nextLevelThreshold = (int)Math.Pow(level, 2) * 100;
        var pointsToNext = nextLevelThreshold - totalPoints;

        double progress = 0.0;
        var span = nextLevelThreshold - currentLevelStart;
        if (span > 0)
        {
            var rawProgress = ((double)(totalPoints - currentLevelStart) / span) * 100.0;
            progress = Math.Max(0.0, Math.Min(100.0, rawProgress));
        }

        return new StudentScoreResponseDto(
            studentId,
            totalPoints,
            level,
            currentLevelStart,
            nextLevelThreshold,
            pointsToNext,
            Math.Round(progress, 2)
        );
    }

    public async Task<StudentScoreResponseDto> GetStudentScoreAsync(string studentId)
    {
        var studentExists = await _db.Students.AnyAsync(s => s.StudentID == studentId);
        if (!studentExists)
        {
            throw new KeyNotFoundException($"Student {studentId} not found");
        }

        var totalPoints = await _db.ScoreTransactions
            .Where(t => t.StudentID == studentId)
            .SumAsync(t => (int?)t.Points) ?? 0;

        return CalculateLevelMetrics(studentId, totalPoints);
    }

    public async Task<ScoreTransactionListResponseDto> GetScoreTransactionsAsync(string studentId, string? sourceType, int page, int limit)
    {
        var query = _db.ScoreTransactions
            .AsNoTracking()
            .Where(t => t.StudentID == studentId);

        if (!string.IsNullOrEmpty(sourceType))
        {
            query = query.Where(t => t.SourceType == sourceType);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(t => new ScoreTransactionResponseDto(
                t.TransactionID,
                t.StudentID,
                t.Points,
                t.Reason,
                t.SourceType,
                t.SourceID,
                t.CreatedAt
            ))
            .ToListAsync();

        return new ScoreTransactionListResponseDto(items, totalCount, page, limit);
    }

    public async Task<ScoreTransactionResponseDto> AddScoreTransactionAsync(string studentId, int points, string reason, string sourceType = "ManualAdjustment", string? sourceId = null)
    {
        if (points == 0)
        {
            throw new ArgumentException("Points cannot be zero", nameof(points));
        }

        var student = await _db.Students.FindAsync(studentId)
            ?? throw new KeyNotFoundException($"Student {studentId} not found");

        var transaction = new ScoreTransaction
        {
            StudentID = studentId,
            Points = points,
            Reason = reason,
            SourceType = sourceType,
            SourceID = sourceId,
            CreatedAt = DateTime.UtcNow
        };

        _db.ScoreTransactions.Add(transaction);
        await _db.SaveChangesAsync();

        if (points > 0)
        {
            await _notificationService.SendNotificationAsync(
                studentId,
                "Points Awarded!",
                $"You have been awarded +{points} points: {reason}",
                "PointsAwarded",
                $"points_awarded:{transaction.TransactionID}"
            );
        }

        await _badgeService.EvaluateStudentBadgesAsync(studentId);

        return new ScoreTransactionResponseDto(
            transaction.TransactionID,
            transaction.StudentID,
            transaction.Points,
            transaction.Reason,
            transaction.SourceType,
            transaction.SourceID,
            transaction.CreatedAt
        );
    }
}

