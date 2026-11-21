using TeamUp.Domain.Enums;

namespace TeamUp.Application.Gamification;

public sealed record NotificationDto(
    int NotificationId,
    int StudentId,
    string Title,
    string Message,
    NotificationType Type,
    bool IsRead,
    DateTime CreatedAt
);

public sealed record CreateNotificationRequest(
    int StudentId,
    string Title,
    string Message,
    NotificationType Type
);
