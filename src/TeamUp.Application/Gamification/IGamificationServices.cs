using TeamUp.Application.Common;
using TeamUp.Domain.Enums;

namespace TeamUp.Application.Gamification;

public interface IGamificationService
{
    StudentScoreDto CalculateLevelMetrics(int studentId, int totalPoints);
    Task<StudentScoreDto> GetStudentScoreAsync(int studentId, CancellationToken ct = default);
    Task<PagedResult<ScoreTransactionDto>> GetScoreTransactionsAsync(int studentId, PageRequest page, CancellationToken ct = default);
    Task<ScoreTransactionDto> AddScoreTransactionAsync(int studentId, int points, string reason, ScoreSourceType sourceType = ScoreSourceType.Manual, CancellationToken ct = default);
}

public interface IBadgeService
{
    Task<IReadOnlyList<string>> EvaluateStudentBadgesAsync(int studentId, CancellationToken ct = default);
    Task<StudentBadgeDto> ManualAwardBadgeAsync(int studentId, int badgeId, CancellationToken ct = default);
    Task<IReadOnlyList<StudentBadgeDto>> GetStudentBadgesAsync(int studentId, CancellationToken ct = default);
    Task<IReadOnlyList<BadgeDto>> GetBadgeCatalogAsync(CancellationToken ct = default);
    Task<BadgeDto> CreateBadgeAsync(CreateBadgeRequest request, CancellationToken ct = default);
    Task<BadgeDto> UpdateBadgeAsync(int badgeId, UpdateBadgeRequest request, CancellationToken ct = default);
    Task DeleteBadgeAsync(int badgeId, CancellationToken ct = default);
}

public interface INotificationService
{
    Task<IReadOnlyList<NotificationDto>> GetNotificationsAsync(int studentId, bool unreadOnly = false, CancellationToken ct = default);
    Task MarkAsReadAsync(int studentId, int notificationId, CancellationToken ct = default);
    Task MarkAllAsReadAsync(int studentId, CancellationToken ct = default);
    Task DeleteNotificationAsync(int studentId, int notificationId, CancellationToken ct = default);
    Task CreateNotificationAsync(int studentId, string title, string message, NotificationType type, CancellationToken ct = default);
}
