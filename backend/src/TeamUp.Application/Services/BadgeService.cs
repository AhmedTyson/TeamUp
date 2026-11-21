using Microsoft.EntityFrameworkCore;
using TeamUp.Application.Common.Exceptions;
using TeamUp.Application.Common.Interfaces;
using TeamUp.Application.Gamification;
using TeamUp.Domain.Entities;
using TeamUp.Domain.Enums;

namespace TeamUp.Application.Services;

public sealed class BadgeService(IAppDbContext db) : IBadgeService
{
    public async Task<IReadOnlyList<string>> EvaluateStudentBadgesAsync(int studentId, CancellationToken ct = default)
    {
        var newlyAwarded = new List<string>();

        var totalPoints = await db.ScoreTransactions
            .Where(t => t.StudentId == studentId)
            .SumAsync(t => (int?)t.Points, ct) ?? 0;

        var eventsAttended = await db.SessionAttendances
            .Include(a => a.Session)
            .Where(a => a.StudentId == studentId)
            .Select(a => a.Session.EventId)
            .Distinct()
            .CountAsync(ct);

        var earnedBadgeIds = await db.StudentBadges
            .Where(sb => sb.StudentId == studentId)
            .Select(sb => sb.BadgeId)
            .ToListAsync(ct);

        var unearnedBadges = await db.Badges
            .Where(b => !earnedBadgeIds.Contains(b.BadgeId))
            .ToListAsync(ct);

        foreach (var badge in unearnedBadges)
        {
            var isEligible = badge.CriteriaType switch
            {
                BadgeCriteriaType.PointsThreshold => totalPoints >= badge.CriteriaValue,
                BadgeCriteriaType.EventCount => eventsAttended >= badge.CriteriaValue,
                _ => false
            };

            if (isEligible)
            {
                db.StudentBadges.Add(new StudentBadge
                {
                    StudentId = studentId,
                    BadgeId = badge.BadgeId,
                    AwardedAt = DateTime.UtcNow,
                });

                // Auto dispatch in-app notification
                db.Notifications.Add(new Notification
                {
                    StudentId = studentId,
                    Title = "Badge Unlocked!",
                    Message = $"Congratulations! You earned the '{badge.Name}' achievement badge.",
                    Type = NotificationType.BadgeEarned,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                });

                newlyAwarded.Add(badge.Name);
            }
        }

        if (newlyAwarded.Count > 0)
        {
            await db.SaveChangesAsync(ct);
        }

        return newlyAwarded;
    }

    public async Task<StudentBadgeDto> ManualAwardBadgeAsync(int studentId, int badgeId, CancellationToken ct = default)
    {
        var student = await db.Students.FirstOrDefaultAsync(s => s.StudentId == studentId, ct)
            ?? throw new NotFoundException($"Student with ID {studentId} was not found.");

        var badge = await db.Badges.FirstOrDefaultAsync(b => b.BadgeId == badgeId, ct)
            ?? throw new NotFoundException($"Badge with ID {badgeId} was not found.");

        var existing = await db.StudentBadges.FirstOrDefaultAsync(
            sb => sb.StudentId == studentId && sb.BadgeId == badgeId, ct);

        if (existing is not null)
            throw new ConflictException("Student has already earned this badge.");

        var award = new StudentBadge
        {
            StudentId = studentId,
            BadgeId = badgeId,
            AwardedAt = DateTime.UtcNow,
        };

        db.StudentBadges.Add(award);

        db.Notifications.Add(new Notification
        {
            StudentId = studentId,
            Title = "Badge Awarded!",
            Message = $"You were awarded the '{badge.Name}' badge by an administrator.",
            Type = NotificationType.BadgeEarned,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
        });

        await db.SaveChangesAsync(ct);

        return new StudentBadgeDto(
            studentId,
            badgeId,
            badge.Name,
            badge.Description,
            badge.IconUrl,
            badge.CriteriaType,
            badge.CriteriaValue,
            award.AwardedAt);
    }

    public async Task<IReadOnlyList<StudentBadgeDto>> GetStudentBadgesAsync(int studentId, CancellationToken ct = default)
    {
        var studentExists = await db.Students.AnyAsync(s => s.StudentId == studentId, ct);
        if (!studentExists)
            throw new NotFoundException($"Student with ID {studentId} was not found.");

        return await db.StudentBadges
            .AsNoTracking()
            .Where(sb => sb.StudentId == studentId)
            .Include(sb => sb.Badge)
            .OrderByDescending(sb => sb.AwardedAt)
            .Select(sb => new StudentBadgeDto(
                sb.StudentId,
                sb.BadgeId,
                sb.Badge.Name,
                sb.Badge.Description,
                sb.Badge.IconUrl,
                sb.Badge.CriteriaType,
                sb.Badge.CriteriaValue,
                sb.AwardedAt))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<BadgeDto>> GetBadgeCatalogAsync(CancellationToken ct = default)
    {
        return await db.Badges
            .AsNoTracking()
            .OrderBy(b => b.BadgeId)
            .Select(b => new BadgeDto(
                b.BadgeId,
                b.Name,
                b.Description,
                b.IconUrl,
                b.CriteriaType,
                b.CriteriaValue))
            .ToListAsync(ct);
    }

    public async Task<BadgeDto> CreateBadgeAsync(CreateBadgeRequest request, CancellationToken ct = default)
    {
        var name = request.Name.Trim();
        if (await db.Badges.AnyAsync(b => b.Name == name, ct))
            throw new ConflictException($"A badge named '{name}' already exists in the catalog.");

        var badge = new Badge
        {
            Name = name,
            Description = request.Description.Trim(),
            IconUrl = request.IconUrl.Trim(),
            CriteriaType = request.CriteriaType,
            CriteriaValue = request.CriteriaValue,
        };

        db.Badges.Add(badge);
        await db.SaveChangesAsync(ct);

        return new BadgeDto(
            badge.BadgeId,
            badge.Name,
            badge.Description,
            badge.IconUrl,
            badge.CriteriaType,
            badge.CriteriaValue);
    }

    public async Task<BadgeDto> UpdateBadgeAsync(int badgeId, UpdateBadgeRequest request, CancellationToken ct = default)
    {
        var badge = await db.Badges.FirstOrDefaultAsync(b => b.BadgeId == badgeId, ct)
            ?? throw new NotFoundException($"Badge with ID {badgeId} was not found.");

        if (request.Name is not null)
        {
            var name = request.Name.Trim();
            if (!string.Equals(badge.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                if (await db.Badges.AnyAsync(b => b.BadgeId != badgeId && b.Name == name, ct))
                    throw new ConflictException($"A badge named '{name}' already exists in the catalog.");
            }
            badge.Name = name;
        }

        if (request.Description is not null) badge.Description = request.Description.Trim();
        if (request.IconUrl is not null) badge.IconUrl = request.IconUrl.Trim();
        if (request.CriteriaType.HasValue) badge.CriteriaType = request.CriteriaType.Value;
        if (request.CriteriaValue.HasValue) badge.CriteriaValue = request.CriteriaValue.Value;

        await db.SaveChangesAsync(ct);

        return new BadgeDto(
            badge.BadgeId,
            badge.Name,
            badge.Description,
            badge.IconUrl,
            badge.CriteriaType,
            badge.CriteriaValue);
    }

    public async Task DeleteBadgeAsync(int badgeId, CancellationToken ct = default)
    {
        var badge = await db.Badges.FirstOrDefaultAsync(b => b.BadgeId == badgeId, ct)
            ?? throw new NotFoundException($"Badge with ID {badgeId} was not found.");

        db.Badges.Remove(badge);
        await db.SaveChangesAsync(ct);
    }
}
