// Date: 2026-11-25
// Purpose: Integration tests verifying student organization creation requests, PlatformAdmin approval with auto-promotion to OrgAdmin + SecurityStamp rotation, rejection, and withdrawal.

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TeamUp.Application.Auth;
using TeamUp.Application.Common;
using TeamUp.Application.Common.Models;
using TeamUp.Application.Organizations;
using TeamUp.Domain.Enums;
using TeamUp.Infrastructure.Persistence;

namespace TeamUp.IntegrationTests;

public class OrganizationCreationRequestTests(TeamUpApiFactory factory) : IClassFixture<TeamUpApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private static async Task<T> ReadDataAsync<T>(HttpResponseMessage res)
    {
        var body = await res.Content.ReadAsStringAsync();
        Assert.True(res.IsSuccessStatusCode, $"Expected 2xx, got {(int)res.StatusCode}: {body}");
        var env = JsonSerializer.Deserialize<BaseResponse<T>>(body, JsonOptions);
        Assert.NotNull(env);
        Assert.True(env.Success);
        Assert.NotNull(env.Data);
        return env.Data;
    }

    [Fact]
    public async Task StudentRequest_AdminApproval_CreatesOrg_PromotesToOrgAdmin_RotatesSecurityStamp_AndAudits()
    {
        var email = $"founder.{Guid.NewGuid():N}@helwan.edu.eg";
        const string password = "FounderPass2026!#Strong";

        using var anonClient = factory.CreateClient();
        var regRes = await anonClient.PostAsJsonAsync("/api/v1/auth/register", new RegisterRequest(
            "Kareem", "Wael", email, password, 3, factory.SeededFacultyId));
        var regAuth = await ReadDataAsync<AuthResponse>(regRes);
        Assert.Equal(nameof(UserRole.Student), regAuth.Student.Role);

        using var studentClient = factory.CreateClient();
        studentClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", regAuth.AccessToken);

        var orgName = $"Pixels Helwan {Guid.NewGuid():N}"[..36];
        var proposal = new CreateOrganizationProposalRequest(
            orgName,
            "Helwan technical student community empowering web & embedded projects.",
            "Technology",
            "https://upload.wikimedia.org/wikipedia/commons/2/21/IEEE_logo.svg",
            OrganizationScope.UniversityBranch,
            factory.SeededUniversityId,
            "Helwan, Cairo",
            null,
            "https://pixels-helwan.org",
            "Launch 2026",
            "Sponsored by faculty student affairs office.");

        var submitRes = await studentClient.PostAsJsonAsync("/api/v1/organizations/requests", proposal);
        Assert.Equal(HttpStatusCode.Created, submitRes.StatusCode);
        var createdReq = await ReadDataAsync<OrganizationCreationRequestDto>(submitRes);
        Assert.Equal(ApplicationStatus.Pending, createdReq.Status);
        Assert.Equal(orgName, createdReq.Name);

        // Duplicate pending request by same student rejected
        var dupRes = await studentClient.PostAsJsonAsync("/api/v1/organizations/requests", proposal);
        Assert.Equal(HttpStatusCode.Conflict, dupRes.StatusCode);

        // SSRF URL rejected
        var ssrfRes = await studentClient.PostAsJsonAsync("/api/v1/organizations/requests", proposal with
        {
            Name = $"SSRF {Guid.NewGuid():N}",
            LogoUrl = "http://169.254.169.254/latest/meta-data/"
        });
        Assert.Equal(HttpStatusCode.BadRequest, ssrfRes.StatusCode);

        // Student cannot access PlatformAdmin queue
        Assert.Equal(HttpStatusCode.Forbidden, (await studentClient.GetAsync("/api/v1/admin/organization-requests")).StatusCode);

        // Different student cannot withdraw requester's proposal
        using var otherStudentClient = factory.CreateClient();
        otherStudentClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", factory.CreateToken(98101, $"other.{Guid.NewGuid():N}@teamup.test", "Student"));
        Assert.Equal(HttpStatusCode.Forbidden,
            (await otherStudentClient.DeleteAsync($"/api/v1/organizations/requests/{createdReq.RequestId}")).StatusCode);

        // PlatformAdmin views queue & approves
        using var adminClient = factory.CreateClient();
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", factory.CreateAdminToken());

        var queueRes = await adminClient.GetAsync($"/api/v1/admin/organization-requests?status=Pending&search={Uri.EscapeDataString(orgName)}");
        var queue = await ReadDataAsync<PagedResult<OrganizationCreationRequestDto>>(queueRes);
        Assert.Contains(queue.Items, r => r.RequestId == createdReq.RequestId);

        var approveRes = await adminClient.PatchAsJsonAsync(
            $"/api/v1/admin/organization-requests/{createdReq.RequestId}/review",
            new ReviewOrganizationProposalRequest(true, "Verified faculty club charter."));
        var approved = await ReadDataAsync<OrganizationCreationRequestDto>(approveRes);

        Assert.Equal(ApplicationStatus.Accepted, approved.Status);
        Assert.NotNull(approved.CreatedOrganizationId);

        // Re-reviewing is blocked with 409 Conflict
        var reReviewRes = await adminClient.PatchAsJsonAsync(
            $"/api/v1/admin/organization-requests/{createdReq.RequestId}/review",
            new ReviewOrganizationProposalRequest(false, "Cannot reverse"));
        Assert.Equal(HttpStatusCode.Conflict, reReviewRes.StatusCode);

        // SecurityStamp rotation invalidates old Student access token (STRIDE THR-E-02)
        var staleCheckRes = await studentClient.GetAsync("/api/v1/organizations/requests/my");
        Assert.Equal(HttpStatusCode.Unauthorized, staleCheckRes.StatusCode);

        // Fresh login reflects promoted OrgAdmin role and President HighBoard membership
        var loginRes = await anonClient.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest(email, password));
        var promotedAuth = await ReadDataAsync<AuthResponse>(loginRes);
        Assert.Equal(nameof(UserRole.OrgAdmin), promotedAuth.Student.Role);

        using var promotedClient = factory.CreateClient();
        promotedClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", promotedAuth.AccessToken);

        var updateOrgRes = await promotedClient.PatchAsJsonAsync(
            $"/api/v1/organizations/{approved.CreatedOrganizationId}",
            new UpdateOrganizationRequest(
                orgName,
                "Updated description by promoted Organization Admin founder.",
                "Technology",
                null,
                OrganizationScope.UniversityBranch,
                factory.SeededUniversityId,
                "Helwan, Cairo",
                null,
                null,
                true,
                "Launch 2026"));
        Assert.Equal(HttpStatusCode.OK, updateOrgRes.StatusCode);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var membership = await db.OrganizationMembers.SingleAsync(
            m => m.OrganizationId == approved.CreatedOrganizationId && m.StudentId == regAuth.Student.StudentId);
        Assert.Equal("President", membership.Role);
        Assert.Equal(OrganizationHierarchyTier.HighBoard, membership.HierarchyTier);

        Assert.True(await db.Notifications.AnyAsync(
            n => n.StudentId == regAuth.Student.StudentId && n.Title == "Organization Request Approved"));
        Assert.True(await db.AuditLogs.AnyAsync(
            a => a.Action == "OrganizationRequestApproved" && a.EntityId == createdReq.RequestId.ToString()));
    }

    [Fact]
    public async Task StudentRequest_AdminRejectionAndOwnerWithdrawal_KeepStudentRole()
    {
        var email = $"reject.{Guid.NewGuid():N}@teamup.edu";
        using var anonClient = factory.CreateClient();
        var regRes = await anonClient.PostAsJsonAsync("/api/v1/auth/register", new RegisterRequest(
            "Salma", "Adel", email, "StudentPass2026!#Strong", 2, factory.SeededFacultyId));
        var auth = await ReadDataAsync<AuthResponse>(regRes);

        using var studentClient = factory.CreateClient();
        studentClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var r1Res = await studentClient.PostAsJsonAsync("/api/v1/organizations/requests", new CreateOrganizationProposalRequest(
            $"Declined Club {Guid.NewGuid():N}"[..36],
            "Description for decline workflow verification.",
            "Leadership",
            null));
        var r1 = await ReadDataAsync<OrganizationCreationRequestDto>(r1Res);

        var r2Res = await studentClient.PostAsJsonAsync("/api/v1/organizations/requests", new CreateOrganizationProposalRequest(
            $"Withdrawn Club {Guid.NewGuid():N}"[..36],
            "Description for withdrawal workflow verification.",
            "Engineering",
            null));
        var r2 = await ReadDataAsync<OrganizationCreationRequestDto>(r2Res);

        using var adminClient = factory.CreateClient();
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", factory.CreateAdminToken());

        var rejectRes = await adminClient.PatchAsJsonAsync(
            $"/api/v1/admin/organization-requests/{r1.RequestId}/review",
            new ReviewOrganizationProposalRequest(false, "Duplicate campus club chapter."));
        var rejected = await ReadDataAsync<OrganizationCreationRequestDto>(rejectRes);
        Assert.Equal(ApplicationStatus.Rejected, rejected.Status);
        Assert.Null(rejected.CreatedOrganizationId);

        // Withdraw second pending request by owner
        var withdrawRes = await studentClient.DeleteAsync($"/api/v1/organizations/requests/{r2.RequestId}");
        Assert.Equal(HttpStatusCode.NoContent, withdrawRes.StatusCode);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var stu = await db.Students.SingleAsync(s => s.StudentId == auth.Student.StudentId);
        Assert.Equal(UserRole.Student, stu.Role);
    }
}
