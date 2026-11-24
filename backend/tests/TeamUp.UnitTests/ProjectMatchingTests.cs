// Date: 2026-11-24
// Purpose: Unit tests verifying Freelancer-style weighted project recommendation scoring and exclusion rules.

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TeamUp.Application.Services;
using TeamUp.Domain.Entities;
using TeamUp.Domain.Enums;
using TeamUp.Infrastructure.Persistence;

namespace TeamUp.UnitTests;

public class ProjectMatchingTests
{
    [Fact]
    public void MandatorySkillMatch_OutranksOptionalSkillMatch_AndInterestOnlyMatch()
    {
        var studentSkills = new Dictionary<int, (string SkillName, ProficiencyLevel Level)>
        {
            [1] = ("React", ProficiencyLevel.Advanced),
            [2] = ("Python", ProficiencyLevel.Intermediate),
        };
        var interests = new[] { "Robotics" };

        var mandatoryResult = ProjectRecommendationService.ComputeMatchScore(
            studentSkills,
            interests,
            [(1, "React", ProficiencyLevel.Intermediate, true), (3, "Docker", ProficiencyLevel.Beginner, false)],
            "Vision Console",
            "React dashboard");

        var optionalResult = ProjectRecommendationService.ComputeMatchScore(
            studentSkills,
            interests,
            [(3, "Docker", ProficiencyLevel.Intermediate, true), (1, "React", ProficiencyLevel.Intermediate, false)],
            "Infra Mesh",
            "Container orchestration");

        var interestOnlyResult = ProjectRecommendationService.ComputeMatchScore(
            studentSkills,
            interests,
            [(3, "Docker", ProficiencyLevel.Intermediate, true)],
            "Robotics Telemetry",
            "Embedded control bus");

        Assert.True(mandatoryResult.Score > optionalResult.Score);
        Assert.True(optionalResult.Score > interestOnlyResult.Score);
        Assert.Equal(10, interestOnlyResult.Score);
    }

    [Fact]
    public void ProficiencyBonus_RewardsMeetingOrExceedingRequiredLevel()
    {
        var juniorSkills = new Dictionary<int, (string, ProficiencyLevel)> { [1] = ("C#", ProficiencyLevel.Beginner) };
        var seniorSkills = new Dictionary<int, (string, ProficiencyLevel)> { [1] = ("C#", ProficiencyLevel.Advanced) };
        var required = new[] { (1, "C#", ProficiencyLevel.Advanced, true) };

        var junior = ProjectRecommendationService.ComputeMatchScore(juniorSkills, [], required, "API Core", "Backend");
        var senior = ProjectRecommendationService.ComputeMatchScore(seniorSkills, [], required, "API Core", "Backend");

        Assert.Equal(80, junior.Score);
        Assert.Equal(90, senior.Score);
    }

    [Fact]
    public void ComputeMatchScore_IdentifiesMatchedMissingSkillsAndInterests()
    {
        var owned = new Dictionary<int, (string, ProficiencyLevel)>
        {
            [1] = ("Python", ProficiencyLevel.Advanced),
            [2] = ("React", ProficiencyLevel.Intermediate)
        };
        var required = new[]
        {
            (1, "Python", ProficiencyLevel.Intermediate, true),
            (3, "Docker", ProficiencyLevel.Beginner, true),
            (2, "React", ProficiencyLevel.Beginner, false)
        };

        var result = ProjectRecommendationService.ComputeMatchScore(
            owned,
            ["Computer Vision", "Finance"],
            required,
            "Computer Vision Traffic Monitor",
            "Live street analytics pipeline");

        Assert.Equal(new[] { "Python", "React" }, result.MatchedSkills);
        Assert.Equal(new[] { "Docker" }, result.MissingSkills);
        Assert.Equal(new[] { "Computer Vision" }, result.MatchedInterests);
        Assert.True(result.Score >= 60);
    }

    [Fact]
    public async Task GetRecommendedProjects_ExcludesFullJoinedCreatedAndAppliedProjects()
    {
        using var conn = new SqliteConnection("DataSource=:memory:");
        conn.Open();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(conn).Options;
        using var db = new AppDbContext(options);
        db.Database.EnsureCreated();

        var uni = new University { Name = "Cairo University", City = "Giza", Country = "Egypt" };
        var faculty = new Faculty { Name = "FCAI", Code = "FCAI", University = uni };
        db.Universities.Add(uni);
        db.Faculties.Add(faculty);
        await db.SaveChangesAsync();

        var targetStudent = new Student { Email = "s1@cu.edu.eg", PasswordHash = "h", FirstName = "A", LastName = "B", AcademicYear = 3, FacultyId = faculty.FacultyId };
        var lead = new Student { Email = "lead@cu.edu.eg", PasswordHash = "h", FirstName = "Lead", LastName = "Dev", AcademicYear = 4, FacultyId = faculty.FacultyId };
        var member2 = new Student { Email = "m2@cu.edu.eg", PasswordHash = "h", FirstName = "M2", LastName = "Dev", AcademicYear = 2, FacultyId = faculty.FacultyId };
        var skill = new Skill { Name = "TypeScript", Category = "Frontend" };
        db.Students.AddRange(targetStudent, lead, member2);
        db.Skills.Add(skill);
        await db.SaveChangesAsync();

        db.StudentSkills.Add(new StudentSkill { StudentId = targetStudent.StudentId, SkillId = skill.SkillId, ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 2 });

        var eligible = new Project { Title = "Eligible Squad", Description = "Needs TS", MaxTeamSize = 4, CreatorId = lead.StudentId, Status = ProjectStatus.Recruiting };
        var ownProject = new Project { Title = "Own Squad", Description = "Creator", MaxTeamSize = 4, CreatorId = targetStudent.StudentId, Status = ProjectStatus.Recruiting };
        var fullProject = new Project { Title = "Full Squad", Description = "No seats", MaxTeamSize = 2, CreatorId = lead.StudentId, Status = ProjectStatus.Recruiting };
        var joinedProject = new Project { Title = "Joined Squad", Description = "Member", MaxTeamSize = 5, CreatorId = lead.StudentId, Status = ProjectStatus.Recruiting };
        var appliedProject = new Project { Title = "Applied Squad", Description = "Pending app", MaxTeamSize = 5, CreatorId = lead.StudentId, Status = ProjectStatus.Recruiting };
        db.Projects.AddRange(eligible, ownProject, fullProject, joinedProject, appliedProject);
        await db.SaveChangesAsync();

        db.ProjectMembers.AddRange(
            new ProjectMember { ProjectId = eligible.ProjectId, StudentId = lead.StudentId, Role = "Team Lead" },
            new ProjectMember { ProjectId = ownProject.ProjectId, StudentId = targetStudent.StudentId, Role = "Team Lead" },
            new ProjectMember { ProjectId = fullProject.ProjectId, StudentId = lead.StudentId, Role = "Team Lead" },
            new ProjectMember { ProjectId = fullProject.ProjectId, StudentId = member2.StudentId, Role = "Member" },
            new ProjectMember { ProjectId = joinedProject.ProjectId, StudentId = lead.StudentId, Role = "Team Lead" },
            new ProjectMember { ProjectId = joinedProject.ProjectId, StudentId = targetStudent.StudentId, Role = "Member" },
            new ProjectMember { ProjectId = appliedProject.ProjectId, StudentId = lead.StudentId, Role = "Team Lead" }
        );
        db.ProjectApplications.Add(new ProjectApplication { ProjectId = appliedProject.ProjectId, StudentId = targetStudent.StudentId, Status = ApplicationStatus.Pending });

        foreach (var p in new[] { eligible, ownProject, fullProject, joinedProject, appliedProject })
        {
            db.ProjectRequiredSkills.Add(new ProjectRequiredSkill { ProjectId = p.ProjectId, SkillId = skill.SkillId, RequiredLevel = ProficiencyLevel.Intermediate, IsMandatory = true });
        }
        await db.SaveChangesAsync();

        var svc = new ProjectRecommendationService(db);
        var results = await svc.GetRecommendedProjectsForStudentAsync(targetStudent.StudentId, maxResults: 10, minMatchPercentage: 1);

        var match = Assert.Single(results);
        Assert.Equal(eligible.ProjectId, match.ProjectId);
        Assert.Equal(3, match.OpenSeats);
        Assert.Equal(90, match.MatchPercentage);
    }
}
