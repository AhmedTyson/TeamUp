namespace TeamUp.Application.Common.Security;

public class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; set; } = "TeamUp";
    public string Audience { get; set; } = "TeamUp.Clients";
    public string Key { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 7;
    public int PasswordResetTokenMinutes { get; set; } = 15;
    public int EmailVerificationTokenMinutes { get; set; } = 60;
}
