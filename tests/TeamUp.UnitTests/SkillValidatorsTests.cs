using TeamUp.Application.Skills;
using TeamUp.Application.Skills.Validators;
using TeamUp.Domain.Enums;

namespace TeamUp.UnitTests;

public class SkillValidatorsTests
{
    private readonly CreateSkillRequestValidator _createValidator = new();
    private readonly UpdateSkillRequestValidator _updateValidator = new();
    private readonly ClaimSkillRequestValidator _claimValidator = new();

    [Fact]
    public void CreateSkill_Valid_Passes()
    {
        var req = new CreateSkillRequest("C#", "Backend", "Modern C# on .NET 10");
        var res = _createValidator.Validate(req);
        Assert.True(res.IsValid);
    }

    [Theory]
    [InlineData("", "Backend")]
    [InlineData("Valid", "")]
    public void CreateSkill_EmptyFields_Fails(string name, string category)
    {
        var req = new CreateSkillRequest(name, category, null);
        var res = _createValidator.Validate(req);
        Assert.False(res.IsValid);
    }

    [Fact]
    public void ClaimSkill_Valid_Passes()
    {
        var req = new ClaimSkillRequest(ProficiencyLevel.Advanced, 3.5m);
        var res = _claimValidator.Validate(req);
        Assert.True(res.IsValid);
    }

    [Fact]
    public void ClaimSkill_NegativeYears_Fails()
    {
        var req = new ClaimSkillRequest(ProficiencyLevel.Beginner, -1m);
        var res = _claimValidator.Validate(req);
        Assert.False(res.IsValid);
        Assert.Contains(res.Errors, e => e.PropertyName == "YearsOfExperience");
    }
}
