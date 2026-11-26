// Date: 2026-11-25
// Purpose: Round-4 flexibility audit tests: gamification rules, notification inbox sizing and the admin
// soft-deleted preview cap come from configuration sections (Gamification, Notifications, Admin) instead of
// compiled-in constants, with the shipped values preserved as defaults.

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TeamUp.Application.Common.Security;
using TeamUp.Application.Services;
using TeamUp.Domain.Entities;
using TeamUp.Domain.Enums;
using TeamUp.Infrastructure.Persistence;

namespace TeamUp.UnitTests;

public sealed class GamificationOptionsTests
{
    [Fact]
    public void Defaults_PreserveShippedEconomy()
    {
        var options = new GamificationOptions();
        Assert.Equal(25, options.PointsPerSession);
        Assert.Equal(50, options.PointsPerCompletedTask);
        Assert.Equal(100, options.PointsPerProjectDelivered);
        Assert.Equal(100, options.LevelStepPoints);
        Assert.Equal(10, options.DigestLeaderboardSize);
    }

    [Fact]
    public void CalculateLevelMetrics_UsesConfiguredLevelStepPoints()
    {
        var service = new GamificationService(
            null!, null, Options.Create(new GamificationOptions { LevelStepPoints = 50 }));

        // step 50: sqrt(500/50) = sqrt(10) = 3.16 -> level 4; level 4 spans 450..800.
        var result = service.CalculateLevelMetrics(1, 500);

        Assert.Equal(4, result.Level);
        Assert.Equal(450, result.CurrentLevelStart);
        Assert.Equal(800, result.NextLevelThreshold);
    }

    [Fact]
    public void CalculateLevelMetrics_InvalidConfiguredStep_FallsBackToDefaultCurve()
    {
        var service = new GamificationService(
            null!, null, Options.Create(new GamificationOptions { LevelStepPoints = 0 }));

        var result = service.CalculateLevelMetrics(1, 100);

        // Default step 100: level 2 at exactly 100 points.
        Assert.Equal(2, result.Level);
        Assert.Equal(100, result.CurrentLevelStart);
        Assert.Equal(400, result.NextLevelThreshold);
    }
}

public sealed class NotificationOptionsTests : IDisposable
{
    private readonly SqliteConnection _conn = new("DataSource=:memory:");
    private readonly int _studentId;

    public NotificationOptionsTests()
    {
        _conn.Open();
        using var db = NewDb();
        db.Database.EnsureCreated();
        var uni = new University { Name = "Cairo University", City = "Giza", Country = "Egypt" };
        var faculty = new Faculty { Name = "Engineering", Code = "ENG", University = uni };
        var student = new Student
        {
            Email = "notif@test.dev",
            PasswordHash = "hash",
            FirstName = "Notif",
            LastName = "Tester",
            AcademicYear = 2,
            Faculty = faculty
        };
        db.Students.Add(student);
        db.SaveChanges();
        _studentId = student.StudentId;

        for (var i = 1; i <= 20; i++)
        {
            db.Notifications.Add(new Notification
            {
                StudentId = _studentId,
                Title = $"N{i}",
                Message = "m",
                Type = NotificationType.EventReminder,
                CreatedAt = DateTime.UtcNow.AddMinutes(i)
            });
        }
        db.SaveChanges();
    }

    private AppDbContext NewDb() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlite(_conn)
        .AddInterceptors(new SoftDeleteInterceptor())
        .Options);

    public void Dispose() => _conn.Dispose();

    [Fact]
    public async Task Inbox_NoTake_UsesConfiguredDefault()
    {
        using var db = NewDb();
        var service = new NotificationService(
            db, Options.Create(new NotificationOptions { DefaultInboxTake = 3, MaxInboxTake = 50 }));

        var result = await service.GetNotificationsAsync(_studentId);

        Assert.Equal(3, result.Count);
        Assert.Equal("N20", result[0].Title);
    }

    [Fact]
    public async Task Inbox_ExplicitTake_ClampedToConfiguredMax()
    {
        using var db = NewDb();
        var service = new NotificationService(
            db, Options.Create(new NotificationOptions { DefaultInboxTake = 5, MaxInboxTake = 7 }));

        var result = await service.GetNotificationsAsync(_studentId, take: 50);

        Assert.Equal(7, result.Count);
    }

    [Fact]
    public async Task Inbox_NoOptions_KeepsShippedDefaults()
    {
        using var db = NewDb();
        var service = new NotificationService(db);

        var result = await service.GetNotificationsAsync(_studentId);

        Assert.Equal(15, result.Count);
    }

    [Fact]
    public async Task Inbox_ConfiguredDefaultAboveMax_ClampedToMax()
    {
        using var db = NewDb();
        var service = new NotificationService(
            db, Options.Create(new NotificationOptions { DefaultInboxTake = 30, MaxInboxTake = 4 }));

        var result = await service.GetNotificationsAsync(_studentId);

        Assert.Equal(4, result.Count);
    }
}

public sealed class AdminOptionsConfigTests
{
    [Fact]
    public void Defaults_PreserveShippedValues()
    {
        var options = new AdminOptions();
        Assert.Equal(200, options.SoftDeletedPreviewLimit);
        Assert.False(options.AllowRuntimePlatformAdminPromotion);
    }
}
