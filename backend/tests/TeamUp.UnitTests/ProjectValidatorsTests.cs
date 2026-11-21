using TeamUp.Application.Projects;
using TeamUp.Application.Projects.Validators;
using TeamUp.Domain.Enums;

namespace TeamUp.UnitTests;

public class ProjectValidatorsTests
{
    private readonly CreateProjectRequestValidator _createProjectValidator = new();
    private readonly ReviewProjectApplicationRequestValidator _reviewAppValidator = new();
    private readonly CreateProjectTaskRequestValidator _createTaskValidator = new();

    [Fact]
    public void CreateProject_ValidRequest_Passes()
    {
        var req = new CreateProjectRequest("AI Drone Platform", "Autonomous drone vision", 4);
        var res = _createProjectValidator.Validate(req);
        Assert.True(res.IsValid);
    }

    [Theory]
    [InlineData("", "Valid description", 4)]
    [InlineData("Valid Title", "", 4)]
    [InlineData("Valid Title", "Valid description", 0)]
    [InlineData("Valid Title", "Valid description", 25)]
    public void CreateProject_InvalidFields_Fails(string title, string description, int teamSize)
    {
        var req = new CreateProjectRequest(title, description, teamSize);
        var res = _createProjectValidator.Validate(req);
        Assert.False(res.IsValid);
    }

    [Theory]
    [InlineData(ApplicationStatus.Accepted, true)]
    [InlineData(ApplicationStatus.Rejected, true)]
    [InlineData(ApplicationStatus.Pending, false)]
    [InlineData(ApplicationStatus.Withdrawn, false)]
    public void ReviewApplication_StatusValidation(ApplicationStatus status, bool expectedValid)
    {
        var req = new ReviewProjectApplicationRequest(status);
        var res = _reviewAppValidator.Validate(req);
        Assert.Equal(expectedValid, res.IsValid);
    }

    [Fact]
    public void CreateTask_EmptyTitle_Fails()
    {
        var req = new CreateProjectTaskRequest("", "Description", null, TaskPriority.Medium);
        var res = _createTaskValidator.Validate(req);
        Assert.False(res.IsValid);
        Assert.Contains(res.Errors, e => e.PropertyName == "Title");
    }
}
