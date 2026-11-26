// Date: 2026-11-26
// Purpose: Unit tests verifying Phase 6 application service fixes (Faculty StudentsCount, configurable ProfileService level step, ProjectDetailDto CoverImageUrl).

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TeamUp.Application.Common.Security;
using TeamUp.Application.Projects;
using TeamUp.Application.Services;
using TeamUp.Application.Universities;
using TeamUp.Domain.Entities;
using TeamUp.Domain.Enums;
using TeamUp.Infrastructure.Persistence;

namespace TeamUp.UnitTests;

public sealed class ServiceHardeningTests : IDisposable
{
    private readonly SqliteConnection _conn = new("DataSource=:memory:");

    public ServiceHardeningTests()
    {
        _conn.Open();
        using var db = CreateDb();
        db.Database.EnsureCreated();
    }

    public void Dispose() => _conn.Dispose();

    private AppDbContext CreateDb() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlite(_conn)
        .Options);

    [Fact]
    public async Task FacultyService_GetByIdAndUpdate_ReturnsAccurateStudentsCount()
    {
        await using var db = CreateDb();
        var uni = new University { Name = "Ain Shams University", City = "Cairo", Country = "Egypt" };
        var fac = new Faculty { Name = "Engineering", Code = "ENG", University = uni };
        db.Faculties.Add(fac);
        await db.SaveChangesAsync();

        db.Students.AddRange(
            new Student { Email = "a@eng.edu.eg", PasswordHash = "h", FirstName = "A", LastName = "B", AcademicYear = 2, FacultyId = fac.FacultyId },
            new Student { Email = "b@eng.edu.eg", PasswordHash = "h", FirstName = "C", LastName = "D", AcademicYear = 3, FacultyId = fac.FacultyId });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var services = new ServiceCollection();
        services.AddHybridCache();
        await using var sp = services.BuildServiceProvider();
        var cache = sp.GetRequiredService<HybridCache>();
        var service = new FacultyService(db, cache, null!);

        var fetched = await service.GetFacultyByIdAsync(fac.FacultyId);
        Assert.Equal(2, fetched.StudentCount);

        var updated = await service.UpdateFacultyAsync(fac.FacultyId, new UpdateFacultyRequest("Faculty of Engineering", "FOE"));
        Assert.Equal(2, updated.StudentCount);
    }

    [Fact]
    public async Task ProfileService_PublicShowcase_UsesConfiguredGamificationLevelStepPoints()
    {
        await using var db = CreateDb();
        var student = new Student
        {
            Email = "showcase@teamup.edu.eg",
            PasswordHash = "h",
            FirstName = "Salma",
            LastName = "Khaled",
            AcademicYear = 3,
            AffiliationType = AffiliationType.IndependentLearner,
            City = "Cairo"
        };
        db.Students.Add(student);
        await db.SaveChangesAsync();

        db.StudentProfiles.Add(new StudentProfile
        {
            StudentId = student.StudentId,
            Bio = "Distributed systems builder.",
            PhotoUrl = "/images/people/salma-khaled.jpg"
        });
        db.ScoreTransactions.Add(new ScoreTransaction
        {
            StudentId = student.StudentId,
            Points = 150,
            SourceType = ScoreSourceType.TaskCompleted,
            Reason = "Adjusted task XP"
        });
        await db.SaveChangesAsync();

        var service = new ProfileService(
            db,
            null!,
            null!,
            Options.Create(new GamificationOptions { LevelStepPoints = 250 }));

        var showcase = await service.GetPublicShowcaseAsync();
        var item = Assert.Single(showcase);
        Assert.Equal(150, item.TotalPoints);
        Assert.Equal(1, item.Level);
    }

    [Fact]
    public async Task ProjectService_GetProjectAsync_IncludesCoverImageUrl()
    {
        await using var db = CreateDb();
        var creator = new Student
        {
            Email = "lead@teamup.edu.eg",
            PasswordHash = "h",
            FirstName = "Omar",
            LastName = "Adel",
            AcademicYear = 4,
            AffiliationType = AffiliationType.IndependentLearner
        };
        db.Students.Add(creator);
        await db.SaveChangesAsync();

        var project = new Project
        {
            Title = "AI Campus Navigator",
            Description = "Campus map and event recommender.",
            MaxTeamSize = 5,
            CreatorId = creator.StudentId,
            Status = ProjectStatus.Recruiting,
            CoverImageUrl = "/api/v1/images/c0e70000-0000-4000-8000-000000000001"
        };
        project.Members.Add(new ProjectMember { StudentId = creator.StudentId, Role = "Team Lead" });
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        var service = new ProjectService(db);
        var detail = await service.GetProjectAsync(project.ProjectId, CancellationToken.None);

        Assert.Equal("/api/v1/images/c0e70000-0000-4000-8000-000000000001", detail.CoverImageUrl);
    }
}
