using TeamUp.Application.Events;
using TeamUp.Application.Events.Validators;
using TeamUp.Application.Gamification;
using TeamUp.Application.Gamification.Validators;
using TeamUp.Domain.Enums;

namespace TeamUp.UnitTests;

public class EventAndBadgeValidatorsTests
{
    private readonly CreateEventRequestValidator _eventValidator = new();
    private readonly CreateBadgeRequestValidator _badgeValidator = new();
    private readonly CreateScoreTransactionRequestValidator _scoreValidator = new();

    [Fact]
    public void CreateEvent_Valid_Passes()
    {
        var req = new CreateEventRequest(
            "AI Hackathon", "Build AI agents", EventType.Hackathon, "Main Hall",
            DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(3), 50, 1);

        var result = _eventValidator.Validate(req);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateEvent_EndDateBeforeStartDate_Fails()
    {
        var req = new CreateEventRequest(
            "AI Hackathon", "Build AI agents", EventType.Hackathon, "Main Hall",
            DateTime.UtcNow.AddDays(3), DateTime.UtcNow.AddDays(1), 50, 1);

        var result = _eventValidator.Validate(req);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "EndDate");
    }

    [Fact]
    public void CreateBadge_Valid_Passes()
    {
        var req = new CreateBadgeRequest("Centurion", "Earn 100 points", "https://cdn.teamup.edu/badge.png", BadgeCriteriaType.PointsThreshold, 100);
        var result = _badgeValidator.Validate(req);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateScoreTransaction_ZeroOrNegativeStudentId_Fails()
    {
        var req = new CreateScoreTransactionRequest(0, 50, "Bonus");
        var result = _scoreValidator.Validate(req);
        Assert.False(result.IsValid);
    }
}
