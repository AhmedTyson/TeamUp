using System.ComponentModel.DataAnnotations;

namespace TeamUp.Api.Models.DTOs;

public record StudentScoreResponseDto(
    string StudentId,
    int TotalPoints,
    int Level,
    int CurrentLevelStart,
    int NextLevelThreshold,
    int PointsToNext,
    double Progress // e.g. 3.64
);

public record ScoreTransactionResponseDto(
    string TransactionId,
    string StudentId,
    int Points,
    string Reason,
    string SourceType,
    string? SourceId,
    DateTime CreatedAt
);

public class ScoreTransactionCreateDto
{
    [Required]
    public string StudentId { get; set; } = string.Empty;

    public int Points { get; set; }

    [Required, StringLength(255, MinimumLength = 3)]
    public string Reason { get; set; } = string.Empty;
}

public record ScoreTransactionListResponseDto(
    List<ScoreTransactionResponseDto> Items,
    int TotalCount,
    int Page,
    int Limit
);

