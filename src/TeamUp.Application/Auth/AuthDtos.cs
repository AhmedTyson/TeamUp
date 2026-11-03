namespace TeamUp.Application.Auth;

public sealed record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    int AcademicYear,
    int FacultyId
);

public sealed record LoginRequest(
    string Email,
    string Password
);

public sealed record RefreshTokenRequest(
    string RefreshToken
);

public sealed record ForgotPasswordRequest(
    string Email
);

public sealed record ForgotPasswordResponse(
    string Message,
    string? ResetToken = null
);

public sealed record ResetPasswordRequest(
    string Email,
    string Token,
    string NewPassword
);

public sealed record StudentSummaryDto(
    int StudentId,
    string FirstName,
    string LastName,
    string Email,
    int AcademicYear,
    string Role,
    int FacultyId,
    string FacultyName,
    string UniversityName
);

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    StudentSummaryDto Student
);
