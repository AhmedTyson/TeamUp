// Date: 2026-11-24
// Purpose: Integration tests covering flexible open-community affiliation, Egyptian hierarchy & chapters/committees, Kanban recruitment, CSV exports, self-status endpoints, public previews, and EventNotification emails.

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using TeamUp.Application.Auth;
using TeamUp.Application.Common;
using TeamUp.Application.Common.Interfaces;
using TeamUp.Application.Common.Models;
using TeamUp.Application.Events;
using TeamUp.Application.Gamification;
using TeamUp.Application.Organizations;
using TeamUp.Application.Students;
using TeamUp.Domain.Common;
using TeamUp.Domain.Entities;
using TeamUp.Domain.Enums;
using TeamUp.Infrastructure.Persistence;
using TeamUp.Infrastructure.Services;

namespace TeamUp.IntegrationTests;

public sealed class FlexibleActivitiesAndHierarchyTests(TeamUpApiFactory factory) : IClassFixture<TeamUpApiFactory>
{
    private static readonly System.Text.Json.JsonSerializerOptions JsonOpts = new(System.Text.Json.JsonSerializerDefaults.Web)
    {
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    private HttpClient Client(string role, int studentId, string? email = null)
    {
        var client = factory.CreateClient();
        var token = factory.CreateToken(studentId, email ?? $"user{studentId}@test.com", role);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static async Task<T> Data<T>(HttpResponseMessage response)
    {
        var envelope = await response.Content.ReadFromJsonAsync<BaseResponse<T>>(JsonOpts);
        Assert.NotNull(envelope);
        Assert.True(envelope.Success, envelope.Message);
        Assert.NotNull(envelope.Data);
        return envelope.Data;
    }

    [Fact]
    public async Task IndependentLearner_RegistersWithoutFacultyId_AndProfileDirectoryLeaderboardSucceed()
    {
        using var anonClient = factory.CreateClient();
        var email = $"indie_{Guid.NewGuid():N}@egypt.tech";
        var registerReq = new RegisterRequest(
            FirstName: "Nouran",
            LastName: "Tarek",
            Email: email,
            Password: "TeamUp!2026Strong",
            AcademicYear: 1,
            FacultyId: null,
            AffiliationType: AffiliationType.IndependentLearner,
            City: "Helwan");

        var regResponse = await anonClient.PostAsJsonAsync("/api/v1/auth/register", registerReq);
        Assert.Equal(HttpStatusCode.Created, regResponse.StatusCode);
        var auth = await Data<AuthResponse>(regResponse);

        Assert.Null(auth.Student.FacultyId);
        Assert.Equal(AffiliationType.IndependentLearner, auth.Student.AffiliationType);
        Assert.Equal("Helwan", auth.Student.City);
        Assert.Equal("Independent Learner", auth.Student.FacultyName);
        Assert.Contains("Helwan", auth.Student.UniversityName);

        using var userClient = factory.CreateClient();
        userClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var meRes = await userClient.GetAsync("/api/v1/students/me");
        Assert.Equal(HttpStatusCode.OK, meRes.StatusCode);
        var me = await Data<StudentDetailDto>(meRes);
        Assert.Null(me.FacultyId);
        Assert.Equal("Helwan", me.City);

        var dirRes = await userClient.GetAsync("/api/v1/students?search=Nouran");
        Assert.Equal(HttpStatusCode.OK, dirRes.StatusCode);
        var dir = await Data<PagedResult<StudentCardDto>>(dirRes);
        Assert.Contains(dir.Items, s => s.StudentId == auth.Student.StudentId && s.FacultyId is null);

        var lbRes = await userClient.GetAsync("/api/v1/gamification/leaderboard?pageSize=50");
        Assert.Equal(HttpStatusCode.OK, lbRes.StatusCode);
        var lb = await Data<PagedResult<LeaderboardEntryDto>>(lbRes);
        Assert.Contains(lb.Items, entry => entry.StudentId == auth.Student.StudentId);
    }

    [Fact]
    public async Task PublicDiscoveryEndpoints_ExposeNonPiiPreview_AndMaskPrivateMeetingUrls()
    {
        using var orgAdminClient = Client("OrgAdmin", 6101, "chair.helwan@teamup.test");
        var orgName = $"IEEE Helwan {Guid.NewGuid():N}";
        var orgRes = await orgAdminClient.PostAsJsonAsync("/api/v1/organizations", new CreateOrganizationRequest(
            Name: orgName,
            Description: "Engineering student branch with technical chapters",
            Category: "Engineering",
            LogoUrl: null,
            Scope: OrganizationScope.UniversityBranch,
            UniversityId: factory.SeededUniversityId,
            City: "Helwan",
            IsRecruiting: true,
            CurrentSeasonName: "Season 2026"));
        Assert.Equal(HttpStatusCode.Created, orgRes.StatusCode);
        var org = await Data<OrganizationDetailDto>(orgRes);

        var secretMeetingUrl = "https://meet.example.com/private-room-xyz";
        var start = DateTime.UtcNow.AddDays(3);
        var evtRes = await orgAdminClient.PostAsJsonAsync("/api/v1/events", new CreateEventRequest(
            Title: $"Hybrid Bootcamp {Guid.NewGuid():N}",
            Description: "Public hybrid workshop",
            Type: EventType.Workshop,
            Location: "Helwan Hall",
            StartDate: start,
            EndDate: start.AddHours(3),
            Capacity: 40,
            OrganizationId: org.OrganizationId,
            DeliveryMode: EventDeliveryMode.Hybrid,
            IsPublic: true,
            MeetingUrl: secretMeetingUrl));
        Assert.Equal(HttpStatusCode.Created, evtRes.StatusCode);
        var evt = await Data<EventDto>(evtRes);

        using var anonClient = factory.CreateClient();

        // Public endpoints work anonymously and NEVER expose emails or MeetingUrl
        var pubOrgsRes = await anonClient.GetAsync("/api/v1/organizations/public?recruitingOnly=true&scope=UniversityBranch");
        Assert.Equal(HttpStatusCode.OK, pubOrgsRes.StatusCode);
        var pubOrgs = await Data<PagedResult<OrganizationSummaryDto>>(pubOrgsRes);
        Assert.Contains(pubOrgs.Items, o => o.OrganizationId == org.OrganizationId && o.IsRecruiting);

        var pubOrgDetailRes = await anonClient.GetAsync($"/api/v1/organizations/public/{org.OrganizationId}");
        Assert.Equal(HttpStatusCode.OK, pubOrgDetailRes.StatusCode);
        var pubOrgDetail = await Data<OrganizationDetailDto>(pubOrgDetailRes);
        Assert.All(pubOrgDetail.Members, m => Assert.Equal(string.Empty, m.Email));

        var pubEvtRes = await anonClient.GetAsync($"/api/v1/events/public/{evt.EventId}");
        Assert.Equal(HttpStatusCode.OK, pubEvtRes.StatusCode);
        var pubEvt = await Data<EventDto>(pubEvtRes);
        Assert.Null(pubEvt.MeetingUrl);
        Assert.Equal(EventDeliveryMode.Hybrid, pubEvt.DeliveryMode);

        // Registered student receives confirmed seat, EventNotification email, MeetingUrl visibility, and my-registrations entry
        using var studentClient = Client("Student", 6102, "attendee6102@teamup.test");
        var regRes = await studentClient.PostAsync($"/api/v1/events/{evt.EventId}/registrations", null);
        Assert.Equal(HttpStatusCode.Created, regRes.StatusCode);

        var authedEvtRes = await studentClient.GetAsync($"/api/v1/events/{evt.EventId}");
        Assert.Equal(HttpStatusCode.OK, authedEvtRes.StatusCode);
        var authedEvt = await Data<EventDto>(authedEvtRes);
        Assert.Equal(secretMeetingUrl, authedEvt.MeetingUrl);

        var myRegsRes = await studentClient.GetAsync("/api/v1/events/my-registrations");
        Assert.Equal(HttpStatusCode.OK, myRegsRes.StatusCode);
        var myRegs = await Data<PagedResult<MyEventRegistrationDto>>(myRegsRes);
        Assert.Contains(myRegs.Items, r => r.EventId == evt.EventId && r.Status == RegistrationStatus.Confirmed);

        var orgParticipantsRes = await orgAdminClient.GetAsync($"/api/v1/organizations/{org.OrganizationId}/participants");
        Assert.Equal(HttpStatusCode.OK, orgParticipantsRes.StatusCode);
        var participants = await Data<List<OrganizationParticipantDto>>(orgParticipantsRes);
        Assert.Contains(participants, p => p.StudentId == 6102 && p.EventsRegisteredCount >= 1);

        // Verify EventNotification.html email was sent via InMemoryEmailSender
        if (factory.Services.GetRequiredService<IEmailSender>() is InMemoryEmailSender sender)
        {
            Assert.Contains(sender.GetSentMessages("attendee6102@teamup.test"), m =>
                m.HtmlBody.Contains("SEAT CONFIRMED", StringComparison.Ordinal));
        }
    }

    [Fact]
    public async Task ChaptersCommitteesHierarchyAndRecruitmentKanban_SupportDelegatedReviewAndCsvExport()
    {
        using var chairClient = Client("OrgAdmin", 6201, "branch.chair6201@teamup.test");
        using var hrClient = Client("Student", 6202, "hr.head6202@teamup.test");
        using var candidateClient = Client("Student", 6203, "candidate6203@teamup.test");

        var orgRes = await chairClient.PostAsJsonAsync("/api/v1/organizations", new CreateOrganizationRequest(
            Name: $"Helwan Robotics Branch {Guid.NewGuid():N}",
            Description: "Student branch with HR and RAS chapters",
            Category: "Engineering",
            LogoUrl: null,
            Scope: OrganizationScope.UniversityBranch,
            UniversityId: factory.SeededUniversityId,
            City: "Helwan",
            IsRecruiting: true,
            CurrentSeasonName: "Fall 2026"));
        Assert.Equal(HttpStatusCode.Created, orgRes.StatusCode);
        var org = await Data<OrganizationDetailDto>(orgRes);

        var rasUnitRes = await chairClient.PostAsJsonAsync($"/api/v1/organizations/{org.OrganizationId}/units",
            new CreateOrganizationUnitRequest("Robotics & Automation", "RAS", "Autonomous robots", OrganizationUnitType.TechnicalChapter, true, 12));
        Assert.Equal(HttpStatusCode.Created, rasUnitRes.StatusCode);
        var rasUnit = await Data<OrganizationUnitDto>(rasUnitRes);

        var hrUnitRes = await chairClient.PostAsJsonAsync($"/api/v1/organizations/{org.OrganizationId}/units",
            new CreateOrganizationUnitRequest("Human Resources", "HR", "Recruitment", OrganizationUnitType.OperationalCommittee, true, 5));
        Assert.Equal(HttpStatusCode.Created, hrUnitRes.StatusCode);
        var hrUnit = await Data<OrganizationUnitDto>(hrUnitRes);

        // Seed HR Head in DB so we verify delegated HR review + 4-tier hierarchy
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.OrganizationMembers.Add(new OrganizationMember
            {
                OrganizationId = org.OrganizationId,
                StudentId = 6202,
                Role = "Head of HR",
                OrganizationUnitId = hrUnit.OrganizationUnitId,
                HierarchyTier = OrganizationHierarchyTier.CommitteeLeadership,
                JoinedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        // Candidate applies to RAS chapter (note formula-injection prefix test in message)
        var applyRes = await candidateClient.PostAsJsonAsync(
            $"/api/v1/organizations/{org.OrganizationId}/applications",
            new CreateMembershipApplicationRequest("=SUM(1+1) Excited to build ROS robots!", rasUnit.OrganizationUnitId, "Fall 2026"));
        Assert.Equal(HttpStatusCode.Created, applyRes.StatusCode);
        var createdApp = await Data<MembershipApplicationDto>(applyRes);
        Assert.Equal(RecruitmentStage.Applied, createdApp.Stage);
        Assert.Equal("RAS", createdApp.UnitCode);

        // Candidate sees their own application via /my-applications cross-device
        var myAppsRes = await candidateClient.GetAsync("/api/v1/organizations/my-applications");
        Assert.Equal(HttpStatusCode.OK, myAppsRes.StatusCode);
        var myApps = await Data<List<MembershipApplicationDto>>(myAppsRes);
        Assert.Contains(myApps, a => a.ApplicationId == createdApp.ApplicationId);

        // Delegated HR Head moves candidate Applied -> HrScreening -> CommitteeInterview -> Accepted with score & notes
        var screenRes = await hrClient.PatchAsJsonAsync(
            $"/api/v1/organizations/{org.OrganizationId}/applications/{createdApp.ApplicationId}/stage",
            new UpdateApplicationStageRequest(RecruitmentStage.HrScreening, 8.5m, "Strong teamwork motivation"));
        Assert.Equal(HttpStatusCode.OK, screenRes.StatusCode);
        var screened = await Data<MembershipApplicationDto>(screenRes);
        Assert.Equal(RecruitmentStage.HrScreening, screened.Stage);
        Assert.Equal(8.5m, screened.EvaluationScore);

        var interviewRes = await hrClient.PatchAsJsonAsync(
            $"/api/v1/organizations/{org.OrganizationId}/applications/{createdApp.ApplicationId}/stage",
            new UpdateApplicationStageRequest(RecruitmentStage.CommitteeInterview, 8.888m, "Passed technical panel", rasUnit.OrganizationUnitId));
        Assert.Equal(HttpStatusCode.OK, interviewRes.StatusCode);
        var interviewed = await Data<MembershipApplicationDto>(interviewRes);
        Assert.Equal(RecruitmentStage.CommitteeInterview, interviewed.Stage);
        Assert.Equal(8.89m, interviewed.EvaluationScore);

        var acceptRes = await hrClient.PatchAsJsonAsync(
            $"/api/v1/organizations/{org.OrganizationId}/applications/{createdApp.ApplicationId}/stage",
            new UpdateApplicationStageRequest(RecruitmentStage.Accepted, 9.2m, "Placed into RAS technical crew", rasUnit.OrganizationUnitId));
        Assert.Equal(HttpStatusCode.OK, acceptRes.StatusCode);
        var accepted = await Data<MembershipApplicationDto>(acceptRes);
        Assert.Equal(ApplicationStatus.Accepted, accepted.Status);
        Assert.Equal(RecruitmentStage.Accepted, accepted.Stage);

        // Verify hierarchy returns 3 distinct staff tiers cleanly
        var hierarchyRes = await chairClient.GetAsync($"/api/v1/organizations/{org.OrganizationId}/hierarchy");
        Assert.Equal(HttpStatusCode.OK, hierarchyRes.StatusCode);
        var hierarchy = await Data<OrganizationHierarchyDto>(hierarchyRes);
        Assert.Contains(hierarchy.HighBoard, m => m.StudentId == 6201);
        Assert.Contains(hierarchy.CommitteeLeadership, m => m.StudentId == 6202 && m.UnitCode == "HR");
        Assert.Contains(hierarchy.OrganizingCrew, m => m.StudentId == 6203 && m.UnitCode == "RAS");

        // Verify CSV export escapes formula injection
        var csvRes = await chairClient.GetAsync($"/api/v1/organizations/{org.OrganizationId}/applications/export.csv");
        Assert.Equal(HttpStatusCode.OK, csvRes.StatusCode);
        Assert.Equal("text/csv", csvRes.Content.Headers.ContentType?.MediaType);
        var csvText = await csvRes.Content.ReadAsStringAsync();
        Assert.Contains("'=SUM(1+1)", csvText, StringComparison.Ordinal);
    }
}
