using TeamUp.Application.Auth;
using TeamUp.Application.Auth.Validators;
using TeamUp.Application.Common.Validation;
using TeamUp.Application.Students;
using TeamUp.Application.Students.Validators;

namespace TeamUp.UnitTests;

public class ValidatorTests
{
    private readonly RegisterRequestValidator _registerValidator = new();
    private readonly UpsertProfileRequestValidator _profileValidator = new();
    private readonly AddInterestRequestValidator _interestValidator = new();

    [Fact]
    public void RegisterValidator_ValidRequest_PassesValidation()
    {
        var req = new RegisterRequest("Omar", "Kamal", "student@university.edu", "P@ssword123!", 2, 1);
        var res = _registerValidator.Validate(req);
        Assert.True(res.IsValid);
        Assert.Empty(res.Errors);
    }

    [Theory]
    [InlineData("", "Email address is required.")]
    [InlineData("not-an-email", "A valid email address is required.")]
    public void RegisterValidator_InvalidEmail_FailsValidation(string email, string expectedMessage)
    {
        var req = new RegisterRequest("Omar", "Kamal", email, "P@ssword123!", 2, 1);
        var res = _registerValidator.Validate(req);
        Assert.False(res.IsValid);
        Assert.Contains(res.Errors, e => e.PropertyName == "Email" && e.ErrorMessage.Contains(expectedMessage));
    }

    [Theory]
    [InlineData("short", "at least 8 characters")]
    [InlineData("nouppercase123!", "uppercase letter")]
    [InlineData("NOLOWERCASE123!", "lowercase letter")]
    [InlineData("NoDigitsInPassword!", "digit")]
    public void RegisterValidator_WeakPassword_FailsValidation(string password, string expectedError)
    {
        var req = new RegisterRequest("Omar", "Kamal", "student@university.edu", password, 2, 1);
        var res = _registerValidator.Validate(req);
        Assert.False(res.IsValid);
        Assert.Contains(res.Errors, e => e.PropertyName == "Password" && e.ErrorMessage.Contains(expectedError));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(8)]
    [InlineData(-1)]
    public void RegisterValidator_InvalidAcademicYear_FailsValidation(int year)
    {
        var req = new RegisterRequest("Omar", "Kamal", "student@university.edu", "P@ssword123!", year, 1);
        var res = _registerValidator.Validate(req);
        Assert.False(res.IsValid);
        Assert.Contains(res.Errors, e => e.PropertyName == "AcademicYear");
    }

    [Theory]
    [InlineData("https://img.local/photo.png", true)]
    [InlineData("/images/people/salma-khaled.jpg", true)]
    [InlineData("not-a-valid-url", false)]
    [InlineData("/images/../secret.jpg", false)]
    [InlineData("//evil.example/a.jpg", false)]
    [InlineData("javascript:alert(1)", false)]
    public void UpsertProfileValidator_PhotoUrl_ValidatesPublicPathsAndHttpUrls(string photoUrl, bool expected)
    {
        var req = new UpsertProfileRequest("My bio", photoUrl, "https://github.com/omar", "https://linkedin.com/in/omar");
        var res = _profileValidator.Validate(req);
        Assert.Equal(expected, res.IsValid);
        if (!expected)
        {
            Assert.Contains(res.Errors, e => e.PropertyName == "PhotoUrl");
        }
    }

    [Fact]
    public void AddInterestValidator_EmptyTag_FailsValidation()
    {
        var req = new AddInterestRequest("");
        var res = _interestValidator.Validate(req);
        Assert.False(res.IsValid);
        Assert.Contains(res.Errors, e => e.PropertyName == "Interest");
    }

    [Fact]
    public void ImageFileValidator_DetectsValidSignatures_AndRejectsSpoofedHeaders()
    {
        Assert.True(ImageFileValidator.TryDetectImageExtension([0xFF, 0xD8, 0xFF, 0xE0], out var jpgExt, out var jpgMime));
        Assert.Equal(".jpg", jpgExt);
        Assert.Equal("image/jpeg", jpgMime);

        Assert.True(ImageFileValidator.TryDetectImageExtension([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A], out var pngExt, out var pngMime));
        Assert.Equal(".png", pngExt);
        Assert.Equal("image/png", pngMime);

        Assert.True(ImageFileValidator.TryDetectImageExtension([0x52, 0x49, 0x46, 0x46, 0x20, 0x00, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50], out var webpExt, out var webpMime));
        Assert.Equal(".webp", webpExt);
        Assert.Equal("image/webp", webpMime);

        Assert.False(ImageFileValidator.TryDetectImageExtension("<svg><script>alert(1)</script></svg>"u8, out _, out _));
        Assert.False(ImageFileValidator.TryDetectImageExtension("<html><body>bad</body></html>"u8, out _, out _));
        Assert.False(ImageFileValidator.TryDetectImageExtension([0xFF, 0xD8], out _, out _));
    }
}
