using TeamUp.Domain.Enums;

namespace TeamUp.Application.Gamification;

public sealed record StudentScoreDto(
    int StudentId,
    int TotalPoints,
    int Level,
    int CurrentLevelStart,
    int NextLevelThreshold,
    int PointsToNext,
    double Progress
);

public sealed record ScoreTransactionDto(
    int TransactionId,
    int StudentId,
    int Points,
    string Reason,
    ScoreSourceType SourceType,
    DateTime CreatedAt
);

public sealed record CreateScoreTransactionRequest(
    int StudentId,
    int Points,
    string Reason,
    ScoreSourceType SourceType = ScoreSourceType.Manual
);
