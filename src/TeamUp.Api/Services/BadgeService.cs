using Microsoft.EntityFrameworkCore;
using TeamUp.Api.Data;
using TeamUp.Api.Data.Entities;
using TeamUp.Api.Models.DTOs;

namespace TeamUp.Api.Services;

public interface IBadgeService
{
    Task<List<string>> EvaluateStudentBadgesAsync(string studentId);
    Task<StudentBadgeResponseDto> ManualAwardBadgeAsync(string studentId, string badgeId);
    Task<List<StudentBadgeResponseDto>> GetStudentBadgesAsync(string studentId);
    Task<List<BadgeResponseDto>> GetBadgeCatalogAsync();
    Task<BadgeResponseDto> CreateBadgeAsync(BadgeCreateDto dto);
    Task<BadgeResponseDto?> UpdateBadgeAsync(string badgeId, BadgeUpdateDto dto);
    Task<bool> SoftDeleteBadgeAsync(string badgeId);
}

public class BadgeService : IBadgeService
{
    private readonly TeamUpDbContext _db;
    private readonly INotificationService _notificationService;

    public BadgeService(TeamUpDbContext db, INotificationService notificationService)
    {
        _db = db;
        _notificationService = notificationService;
    }

    public async Task<List<string>> EvaluateStudentBadgesAsync(string studentId)
    {
        var newlyAwarded = new List<string>();

        // 1. TotalPoints
        var totalPoints = await _db.ScoreTransactions
            .Where(t => t.StudentID == studentId)
            .SumAsync(t => (int?)t.Points) ?? 0;

        // 2. EventsAttended (Distinct Events attended via non-revoked session attendance)
        var eventsAttended = await _db.SessionAttendances
            .Include(a => a.Session)
            .Where(a => a.StudentID == studentId && !a.IsRevoked)
            .Select(a => a.Session.EventID)
            .Distinct()
            .CountAsync();

        var earnedBadgeIds = await _db.StudentBadges
            .Where(sb => sb.StudentID == studentId)
            .Select(sb => sb.BadgeID)
            .ToListAsync();

        var unearnedBadges = await _db.Badges
            .Where(b => !b.IsDeleted && b.CriteriaType != "Manual" && !earnedBadgeIds.Contains(b.BadgeID))
            .ToListAsync();

        var now = DateTime.UtcNow;

        foreach (var badge in unearnedBadges)
        {
            var isEligible = badge.CriteriaType switch
            {
                "TotalPoints" => totalPoints >= badge.CriteriaValue,
                "EventsAttended" => eventsAttended >= badge.CriteriaValue,
                _ => false
            };

            if (isEligible)
            {
                _db.StudentBadges.Add(new StudentBadge
                {
                    StudentID = studentId,
                    BadgeID = badge.BadgeID,
                    AwardedAt = now
                });

                newlyAwarded.Add(badge.Name);

                await _notificationService.SendNotificationAsync(
                    studentId,
                    "Badge Unlocked!",
                    $"Congratulations! You unlocked the '{badge.Name}' badge.",
                    "BadgeUnlocked",
                    $"badge_unlocked:{studentId}:{badge.BadgeID}"
                );
            }
        }

        if (newlyAwarded.Any())
        {
            await _db.SaveChangesAsync();
        }

        return newlyAwarded;
    }

    public async Task<StudentBadgeResponseDto> ManualAwardBadgeAsync(string studentId, string badgeId)
    {
        var badge = await _db.Badges.FindAsync(badgeId)
            ?? throw new KeyNotFoundException("Badge not found");

        var student = await _db.Students.FindAsync(studentId)
            ?? throw new KeyNotFoundException("Student not found");

        var existing = await _db.StudentBadges.FindAsync(studentId, badgeId);
        if (existing != null)
        {
            throw new InvalidOperationException("Badge already awarded to this student");
        }

        var now = DateTime.UtcNow;
        var studentBadge = new StudentBadge
        {
            StudentID = studentId,
            BadgeID = badgeId,
            AwardedAt = now
        };

        _db.StudentBadges.Add(studentBadge);
        await _db.SaveChangesAsync();

        await _notificationService.SendNotificationAsync(
            studentId,
            "Badge Awarded!",
            $"You have been awarded the '{badge.Name}' badge by an administrator.",
            "BadgeUnlocked",
            $"badge_unlocked:{studentId}:{badgeId}"
        );

        return new StudentBadgeResponseDto(
            studentId,
            badgeId,
            badge.Name,
            badge.Description,
            badge.IconURL,
            badge.CriteriaType,
            badge.CriteriaValue,
            now
        );
    }

    public async Task<List<StudentBadgeResponseDto>> GetStudentBadgesAsync(string studentId)
    {
        return await _db.StudentBadges
            .Include(sb => sb.Badge)
            .Where(sb => sb.StudentID == studentId)
            .OrderByDescending(sb => sb.AwardedAt)
            .Select(sb => new StudentBadgeResponseDto(
                sb.StudentID,
                sb.BadgeID,
                sb.Badge.Name,
                sb.Badge.Description,
                sb.Badge.IconURL,
                sb.Badge.CriteriaType,
                sb.Badge.CriteriaValue,
                sb.AwardedAt
            ))
            .ToListAsync();
    }

    public async Task<List<BadgeResponseDto>> GetBadgeCatalogAsync()
    {
        return await _db.Badges
            .AsNoTracking()
            .Where(b => !b.IsDeleted)
            .OrderBy(b => b.CriteriaType)
            .ThenBy(b => b.CriteriaValue)
            .Select(b => new BadgeResponseDto(
                b.BadgeID,
                b.Name,
                b.Description,
                b.IconURL,
                b.CriteriaType,
                b.CriteriaValue,
                b.IsDeleted,
                b.CreatedAt
            ))
            .ToListAsync();
    }

    public async Task<BadgeResponseDto> CreateBadgeAsync(BadgeCreateDto dto)
    {
        var existing = await _db.Badges.AnyAsync(b => b.Name == dto.Name);
        if (existing)
        {
            throw new InvalidOperationException("Badge with this name already exists");
        }

        var badge = new Badge
        {
            Name = dto.Name,
            Description = dto.Description,
            IconURL = dto.IconUrl,
            CriteriaType = dto.CriteriaType,
            CriteriaValue = dto.CriteriaValue,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.Badges.Add(badge);
        await _db.SaveChangesAsync();

        return new BadgeResponseDto(
            badge.BadgeID,
            badge.Name,
            badge.Description,
            badge.IconURL,
            badge.CriteriaType,
            badge.CriteriaValue,
            badge.IsDeleted,
            badge.CreatedAt
        );
    }

    public async Task<BadgeResponseDto?> UpdateBadgeAsync(string badgeId, BadgeUpdateDto dto)
    {
        var badge = await _db.Badges.FirstOrDefaultAsync(b => b.BadgeID == badgeId && !b.IsDeleted);
        if (badge == null) return null;

        if (dto.Name != null) badge.Name = dto.Name;
        if (dto.Description != null) badge.Description = dto.Description;
        if (dto.IconUrl != null) badge.IconURL = dto.IconUrl;
        if (dto.CriteriaType != null) badge.CriteriaType = dto.CriteriaType;
        if (dto.CriteriaValue.HasValue) badge.CriteriaValue = dto.CriteriaValue.Value;

        await _db.SaveChangesAsync();

        return new BadgeResponseDto(
            badge.BadgeID,
            badge.Name,
            badge.Description,
            badge.IconURL,
            badge.CriteriaType,
            badge.CriteriaValue,
            badge.IsDeleted,
            badge.CreatedAt
        );
    }

    public async Task<bool> SoftDeleteBadgeAsync(string badgeId)
    {
        var badge = await _db.Badges.FirstOrDefaultAsync(b => b.BadgeID == badgeId && !b.IsDeleted);
        if (badge == null) return false;

        badge.IsDeleted = true;
        await _db.SaveChangesAsync();
        return true;
    }
}

