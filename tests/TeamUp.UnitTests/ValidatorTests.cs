using TeamUp.Application.Auth;
using TeamUp.Application.Auth.Validators;
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
        var req = new RegisterRequest("student@university.edu", "P@ssword123!", "Omar", "Kamal", 2, 1);
        var res = _registerValidator.Validate(req);
        Assert.True(res.IsValid);
        Assert.Empty(res.Errors);
    }

    [Theory]
    [InlineData("", "Email address is required.")]
    [InlineData("not-an-email", "A valid email address is required.")]
    public void RegisterValidator_InvalidEmail_FailsValidation(string email, string expectedMessage)
    {
        var req = new RegisterRequest(email, "P@ssword123!", "Omar", "Kamal", 2, 1);
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
        var req = new RegisterRequest("student@university.edu", password, "Omar", "Kamal", 2, 1);
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
        var req = new RegisterRequest("student@university.edu", "P@ssword123!", "Omar", "Kamal", year, 1);
        var res = _registerValidator.Validate(req);
        Assert.False(res.IsValid);
        Assert.Contains(res.Errors, e => e.PropertyName == "AcademicYear");
    }

    [Fact]
    public void UpsertProfileValidator_ValidUrls_PassesValidation()
    {
        var req = new UpsertProfileRequest("My bio", "https://img.local/photo.png", "https://github.com/omar", "https://linkedin.com/in/omar");
        var res = _profileValidator.Validate(req);
        Assert.True(res.IsValid);
    }

    [Fact]
    public void UpsertProfileValidator_InvalidUrl_FailsValidation()
    {
        var req = new UpsertProfileRequest("My bio", "not-a-valid-url", null, null);
        var res = _profileValidator.Validate(req);
        Assert.False(res.IsValid);
        Assert.Contains(res.Errors, e => e.PropertyName == "PhotoUrl");
    }

    [Fact]
    public void AddInterestValidator_EmptyTag_FailsValidation()
    {
        var req = new AddInterestRequest("");
        var res = _interestValidator.Validate(req);
        Assert.False(res.IsValid);
        Assert.Contains(res.Errors, e => e.PropertyName == "Interest");
    }
}
