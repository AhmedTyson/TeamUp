using System.ComponentModel.DataAnnotations;

namespace TeamUp.Api.Models.DTOs;

public record AttendanceVerifyRequestDto(
    [Required] string StudentId,
    [Required, RegularExpression("^(QRCode|RFID|Manual)$")] string VerificationMethod
);

public record SessionAttendanceResponseDto(
    string AttendanceId,
    string SessionId,
    string StudentId,
    DateTime CheckInTime,
    string VerificationMethod,
    bool IsRevoked,
    DateTime? RevokedAt = null,
    int? PointsAwarded = null,
    string? StudentName = null,
    string? StudentEmail = null
);

public record SessionAttendanceListResponseDto(
    List<SessionAttendanceResponseDto> Items,
    int TotalCount,
    int Page,
    int Limit
);

