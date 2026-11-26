// Date: 2026-11-24
// Purpose: Integration tests verifying student email opt-in preference persistence, opt-out filtering, and rendered digest links.

using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using TeamUp.Application.Auth;
using TeamUp.Application.Common.Models;
using TeamUp.Application.Gamification;
using TeamUp.Application.Projects;
using TeamUp.Application.Students;
using TeamUp.Domain.Entities;
using TeamUp.Domain.Enums;
using TeamUp.Infrastructure.Persistence;
using TeamUp.Infrastructure.Services;

namespace TeamUp.IntegrationTests;

public class CommunicationsAndPreferencesTests(TeamUpApiFactory factory) : IClassFixture<TeamUpApiFactory>
{
    private readonly ApiHelper _api = new(factory);

    [Fact]
    public async Task RegisterAndProfilePatch_PersistEmailNotificationAndDigestOptInFlags()
    {
        var email = $"prefs_{Guid.NewGuid():N}@teamup.test";
        var anon = factory.CreateClient();
        var regRes = await anon.PostAsJsonAsync(
            "/api/v1/auth/register",
            new RegisterRequest(
                "OptIn",
                "Student",
                email,
                "P@ssword123!",
                3,
                factory.SeededFacultyId,
                EmailNotificationsEnabled: true,
                ReceiveLeaderboardDigests: true,
                ReceiveProjectMatchEmails: false),
            ApiHelper.Json);

        Assert.Equal(HttpStatusCode.Created, regRes.StatusCode);
        var auth = await ApiHelper.DataAsync<AuthResponse>(regRes);
        var client = _api.Authed(auth.AccessToken);

        var meInitial = await ApiHelper.DataAsync<StudentDetailDto>(await client.GetAsync("/api/v1/students/me"));
        Assert.True(meInitial.EmailNotificationsEnabled);
        Assert.True(meInitial.ReceiveLeaderboardDigests);
        Assert.False(meInitial.ReceiveProjectMatchEmails);

        var patchRes = await client.PatchAsJsonAsync(
            "/api/v1/students/me",
            new UpdateStudentRequest(null, null, null, null, EmailNotificationsEnabled: false, ReceiveLeaderboardDigests: false, ReceiveProjectMatchEmails: true),
            ApiHelper.Json);
        Assert.Equal(HttpStatusCode.OK, patchRes.StatusCode);

        var meUpdated = await ApiHelper.DataAsync<StudentDetailDto>(await client.GetAsync("/api/v1/students/me"));
        Assert.False(meUpdated.EmailNotificationsEnabled);
        Assert.False(meUpdated.ReceiveLeaderboardDigests);
        Assert.True(meUpdated.ReceiveProjectMatchEmails);
    }

    [Fact]
    public async Task LeaderboardDigest_SkipsOptedOutStudents_AndRendersTopStandingsAndNews()
    {
        var (_, optInId, optInEmail, _) = await _api.RegisterAsync("lb_on");
        var (_, _, digestOffEmail, _) = await _api.RegisterAsync("lb_off");
        var (_, _, masterOffEmail, _) = await _api.RegisterAsync("master_off");

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var normalizedDigestOff = digestOffEmail.ToLowerInvariant();
            var normalizedMasterOff = masterOffEmail.ToLowerInvariant();
            var sOff = db.Students.Single(s => s.Email == normalizedDigestOff);
            sOff.ReceiveLeaderboardDigests = false;
            var sMaster = db.Students.Single(s => s.Email == normalizedMasterOff);
            sMaster.EmailNotificationsEnabled = false;

            db.ScoreTransactions.Add(new ScoreTransaction
            {
                StudentId = optInId,
                Points = 1250,
                Reason = "Integration benchmark",
                SourceType = ScoreSourceType.Manual
            });
            await db.SaveChangesAsync();

            var dispatch = scope.ServiceProvider.GetRequiredService<IDigestDispatchService>();
            var result = await dispatch.DispatchLeaderboardDigestAsync(new LeaderboardDigestRequest(
                PeriodLabel: "QA ISSUE 2026",
                NewsItems:
                [
                    new CampusNewsItem("EVENTS", "Cairo Robotics Demo Night", "Submit hardware prototypes by Friday.")
                ],
                DryRun: false));

            Assert.True(result.SentCount >= 1);

            var sender = scope.ServiceProvider.GetRequiredService<InMemoryEmailSender>();
            var sentOptIn = Assert.Single(sender.GetSentMessages(optInEmail));
            Assert.Contains("Cairo Robotics Demo Night", sentOptIn.HtmlBody);
            Assert.Contains("/leaderboard", sentOptIn.HtmlBody);
            Assert.Empty(sender.GetSentMessages(digestOffEmail));
            Assert.Empty(sender.GetSentMessages(masterOffEmail));
        }
    }

    [Fact]
    public async Task ProjectMatchDigest_SkipsOptedOutStudents_AndIncludesValidProjectApplyLinks()
    {
        var (leadClient, leadId, _, _) = await _api.RegisterAsync("match_lead");
        var (_, optInId, optInEmail, _) = await _api.RegisterAsync("match_on");
        var (_, optOutId, optOutEmail, _) = await _api.RegisterAsync("match_off");

        var project = await _api.CreateProjectAsync(leadClient, capacity: 5);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var skill = new Skill { Name = $"GraphQL_{Guid.NewGuid():N}", Category = "Backend" };
        db.Skills.Add(skill);
        await db.SaveChangesAsync();

        db.ProjectRequiredSkills.Add(new ProjectRequiredSkill
        {
            ProjectId = project.ProjectId,
            SkillId = skill.SkillId,
            RequiredLevel = ProficiencyLevel.Intermediate,
            IsMandatory = true
        });
        db.StudentSkills.AddRange(
            new StudentSkill { StudentId = optInId, SkillId = skill.SkillId, ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 2 },
            new StudentSkill { StudentId = optOutId, SkillId = skill.SkillId, ProficiencyLevel = ProficiencyLevel.Advanced, YearsOfExperience = 2 });

        var optedOutStudent = db.Students.Single(s => s.StudentId == optOutId);
        optedOutStudent.ReceiveProjectMatchEmails = false;
        await db.SaveChangesAsync();

        var dispatch = scope.ServiceProvider.GetRequiredService<IDigestDispatchService>();
        var result = await dispatch.DispatchProjectMatchDigestAsync(new ProjectMatchDigestRequest(
            MaxMatchesPerStudent: 5,
            MinMatchPercentage: 50,
            DryRun: false));

        Assert.True(result.SentCount >= 1);
        Assert.NotEqual(leadId, optInId);

        var sender = scope.ServiceProvider.GetRequiredService<InMemoryEmailSender>();
        var sent = Assert.Single(sender.GetSentMessages(optInEmail));
        Assert.Contains($"/projects/{project.ProjectId}", sent.HtmlBody);
        Assert.Contains(skill.Name, sent.HtmlBody);
        Assert.Empty(sender.GetSentMessages(optOutEmail));
    }
}
