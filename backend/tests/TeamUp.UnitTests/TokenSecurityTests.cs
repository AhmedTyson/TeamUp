using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TeamUp.Application.Common.Security;
using TeamUp.Infrastructure.Services;

namespace TeamUp.UnitTests;

public class TokenSecurityTests
{
    private readonly JwtTokenService _jwtService;
    private readonly InMemoryEmailSender _emailSender;

    public TokenSecurityTests()
    {
        var options = Options.Create(new JwtOptions
        {
            Key = "TestSuperSecretKeyForTeamUpUnitTesting1234567890!",
            Issuer = "TeamUpTest",
            Audience = "TeamUpTestAudience",
            AccessTokenMinutes = 15,
            RefreshTokenDays = 7
        });

        _jwtService = new JwtTokenService(options);
        _emailSender = new InMemoryEmailSender(NullLogger<InMemoryEmailSender>.Instance);
    }

    [Fact]
    public void HashToken_ProducesDeterministicSha256Hex()
    {
        var rawToken = "my_sample_refresh_token_string_value";
        var hash1 = _jwtService.HashToken(rawToken);
        var hash2 = _jwtService.HashToken(rawToken);

        Assert.Equal(hash1, hash2);
        Assert.Equal(64, hash1.Length); // 256 bits = 64 hex characters
        Assert.NotEqual(rawToken, hash1);
    }

    [Fact]
    public void HashToken_DifferentInputs_ProduceDifferentHashes()
    {
        var tokenA = _jwtService.GenerateRefreshToken();
        var tokenB = _jwtService.GenerateRefreshToken();

        var hashA = _jwtService.HashToken(tokenA);
        var hashB = _jwtService.HashToken(tokenB);

        Assert.NotEqual(hashA, hashB);
    }

    [Fact]
    public async Task EmailSender_StoresAndRetrievesDispatchedResetToken()
    {
        var email = "student@test.edu";
        var token = "secret_reset_token_xyz";

        await _emailSender.SendPasswordResetEmailAsync(email, token);
        var retrieved = _emailSender.GetLastResetToken(email);

        Assert.Equal(token, retrieved);
    }
}
