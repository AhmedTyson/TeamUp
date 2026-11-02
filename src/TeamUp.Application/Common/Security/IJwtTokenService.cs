namespace TeamUp.Application.Common.Security;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAt) GenerateAccessToken(int studentId, string email, string role, string fullName);
    string GenerateRefreshToken();
}
