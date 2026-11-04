namespace TeamUp.Api.Models.DTOs;

public record NotificationResponseDto(
    string NotificationId,
    string StudentId,
    string Title,
    string Message,
    string Type, // EventRegistration, AttendanceVerified, PointsAwarded, BadgeUnlocked
    bool IsRead,
    bool IsDeleted,
    DateTime CreatedAt
);

public record NotificationListResponseDto(
    List<NotificationResponseDto> Items,
    int UnreadCount,
    int TotalCount,
    int Page,
    int Limit
);

