namespace TeamUp.Application.Auth;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest req, CancellationToken ct = default);
    Task<AuthResponse> LoginAsync(LoginRequest req, CancellationToken ct = default);
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest req, CancellationToken ct = default);
    Task LogoutAsync(int studentId, CancellationToken ct = default);
}
