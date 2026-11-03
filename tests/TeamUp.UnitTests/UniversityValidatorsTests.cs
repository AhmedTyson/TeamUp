using TeamUp.Application.Universities;
using TeamUp.Application.Universities.Validators;

namespace TeamUp.UnitTests;

public class UniversityValidatorsTests
{
    private readonly CreateUniversityRequestValidator _createUniValidator = new();
    private readonly CreateFacultyRequestValidator _createFacultyValidator = new();

    [Fact]
    public void CreateUniversity_Valid_Passes()
    {
        var req = new CreateUniversityRequest("Ain Shams University", "Cairo", "Egypt", "https://asu.edu.eg");
        var res = _createUniValidator.Validate(req);
        Assert.True(res.IsValid);
    }

    [Theory]
    [InlineData("", "Cairo", "Egypt")]
    [InlineData("ASU", "", "Egypt")]
    [InlineData("ASU", "Cairo", "")]
    public void CreateUniversity_MissingFields_Fails(string name, string city, string country)
    {
        var req = new CreateUniversityRequest(name, city, country, null);
        var res = _createUniValidator.Validate(req);
        Assert.False(res.IsValid);
    }

    [Fact]
    public void CreateUniversity_InvalidWebsite_Fails()
    {
        var req = new CreateUniversityRequest("ASU", "Cairo", "Egypt", "not-a-valid-url");
        var res = _createUniValidator.Validate(req);
        Assert.False(res.IsValid);
        Assert.Contains(res.Errors, e => e.PropertyName == "Website");
    }

    [Fact]
    public void CreateFaculty_Valid_Passes()
    {
        var req = new CreateFacultyRequest("Faculty of Science", "SCI");
        var res = _createFacultyValidator.Validate(req);
        Assert.True(res.IsValid);
    }

    [Theory]
    [InlineData("", "ENG")]
    [InlineData("Engineering", "")]
    [InlineData("Engineering", "ENG CODE WITH SPACES")]
    [InlineData("Engineering", "ENG@#!")]
    public void CreateFaculty_Invalid_Fails(string name, string code)
    {
        var req = new CreateFacultyRequest(name, code);
        var res = _createFacultyValidator.Validate(req);
        Assert.False(res.IsValid);
    }
}
