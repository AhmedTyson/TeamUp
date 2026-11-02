namespace TeamUp.Application.Common.Security;

public class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; set; } = "TeamUp";
    public string Audience { get; set; } = "TeamUp.Clients";
    public string Key { get; set; } = null!;
    public int AccessTokenMinutes { get; set; } = 60;
    public int RefreshTokenDays { get; set; } = 7;
}
