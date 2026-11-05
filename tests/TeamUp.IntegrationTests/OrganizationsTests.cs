using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TeamUp.Application.Common;
using TeamUp.Application.Common.Models;
using TeamUp.Application.Organizations;
using TeamUp.Domain.Entities;
using TeamUp.Infrastructure.Persistence;

namespace TeamUp.IntegrationTests;

public sealed class OrganizationsTests(TeamUpApiFactory factory) : IClassFixture<TeamUpApiFactory>
{
    private HttpClient Client(string role, int studentId = 100)
    {
        var client = factory.CreateClient();
        var token = factory.CreateToken(studentId, $"user{studentId}@test.com", role);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static readonly System.Text.Json.JsonSerializerOptions JsonOpts = new(System.Text.Json.JsonSerializerDefaults.Web)
    {
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    private static async Task<T> Data<T>(HttpResponseMessage response)
    {
        var envelope = await response.Content.ReadFromJsonAsync<BaseResponse<T>>(JsonOpts);
        Assert.NotNull(envelope);
        Assert.True(envelope.Success);
        Assert.NotNull(envelope.Data);
        return envelope.Data;
    }

    [Fact]
    public async Task Organizations_ListRequiresAuthentication()
    {
        using var client = factory.CreateClient();
        var response = await client.GetAsync("/api/v1/organizations");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Student_CannotCreateOrganization()
    {
        using var client = Client("Student");
        var request = new CreateOrganizationRequest("Student Club", "A student club", "Technology", null);
        var response = await client.PostAsJsonAsync("/api/v1/organizations", request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task OrganizationCrudAndMemberManagement_WorkWithSoftDelete()
    {
        using var client = Client("OrgAdmin");
        var uniqueName = $"Org_{Guid.NewGuid():N}";
        var createRequest = new CreateOrganizationRequest(uniqueName, "Organization description", "Technology", "https://example.com/logo.png");
        var createResponse = await client.PostAsJsonAsync("/api/v1/organizations", createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await Data<OrganizationDetailDto>(createResponse);

        var duplicateResponse = await client.PostAsJsonAsync("/api/v1/organizations", createRequest);
        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);

        var updateRequest = new UpdateOrganizationRequest(uniqueName + " Updated", "Updated description", "Community", null);
        var updateResponse = await client.PatchAsJsonAsync($"/api/v1/organizations/{created.OrganizationId}", updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await Data<OrganizationDetailDto>(updateResponse);
        Assert.Equal(uniqueName + " Updated", updated.Name);

        const int memberId = 8123;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Students.Add(new Student
            {
                StudentId = memberId,
                Email = $"member{memberId}@test.com",
                PasswordHash = "test-hash",
                FirstName = "Test",
                LastName = "Member",
                AcademicYear = 2,
                FacultyId = factory.SeededFacultyId
            });
            db.OrganizationMembers.Add(new OrganizationMember
            {
                OrganizationId = created.OrganizationId,
                StudentId = memberId,
                Role = "Member"
            });
            await db.SaveChangesAsync();
        }

        var membersResponse = await client.GetAsync($"/api/v1/organizations/{created.OrganizationId}/members");
        Assert.Equal(HttpStatusCode.OK, membersResponse.StatusCode);
        var members = await Data<List<OrganizationMemberDto>>(membersResponse);
        Assert.Contains(members, x => x.StudentId == memberId);

        var roleResponse = await client.PatchAsJsonAsync(
            $"/api/v1/organizations/{created.OrganizationId}/members/{memberId}",
            new UpdateOrganizationMemberRequest("Head of PR"));
        Assert.Equal(HttpStatusCode.OK, roleResponse.StatusCode);
        var updatedMember = await Data<OrganizationMemberDto>(roleResponse);
        Assert.Equal("Head of PR", updatedMember.Role);

        var removeResponse = await client.DeleteAsync($"/api/v1/organizations/{created.OrganizationId}/members/{memberId}");
        Assert.Equal(HttpStatusCode.NoContent, removeResponse.StatusCode);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var deleted = await db.OrganizationMembers.IgnoreQueryFilters()
                .SingleAsync(x => x.OrganizationId == created.OrganizationId && x.StudentId == memberId);
            Assert.NotNull(deleted.DeletedAt);
        }

        var deleteResponse = await client.DeleteAsync($"/api/v1/organizations/{created.OrganizationId}");
        Assert.Equal(HttpStatusCode.Forbidden, deleteResponse.StatusCode);
    }


    [Fact]
    public async Task MembershipApplication_AcceptanceCreatesOrganizationMember()
    {
        const int applicantId = 8124;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Students.Add(new Student
            {
                StudentId = applicantId,
                Email = $"applicant{applicantId}@test.com",
                PasswordHash = "test-hash",
                FirstName = "New",
                LastName = "Applicant",
                AcademicYear = 1,
                FacultyId = factory.SeededFacultyId
            });
            await db.SaveChangesAsync();
        }

        using var admin = Client("OrgAdmin", 901);
        using var applicant = Client("Student", applicantId);
        var organizationRequest = new CreateOrganizationRequest($"ApplyOrg_{Guid.NewGuid():N}", "Description", "Technology", null);
        var organizationResponse = await admin.PostAsJsonAsync("/api/v1/organizations", organizationRequest);
        Assert.Equal(HttpStatusCode.Created, organizationResponse.StatusCode);
        var organization = await Data<OrganizationDetailDto>(organizationResponse);

        var applicationResponse = await applicant.PostAsJsonAsync(
            $"/api/v1/organizations/{organization.OrganizationId}/applications",
            new CreateMembershipApplicationRequest("I want to join."));
        Assert.Equal(HttpStatusCode.Created, applicationResponse.StatusCode);
        var application = await Data<MembershipApplicationDto>(applicationResponse);
        Assert.Equal(TeamUp.Domain.Enums.ApplicationStatus.Pending, application.Status);

        var duplicateResponse = await applicant.PostAsJsonAsync(
            $"/api/v1/organizations/{organization.OrganizationId}/applications",
            new CreateMembershipApplicationRequest("Duplicate."));
        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);

        var reviewResponse = await admin.PatchAsJsonAsync(
            $"/api/v1/organizations/{organization.OrganizationId}/applications/{application.ApplicationId}",
            new ReviewMembershipApplicationRequest(TeamUp.Domain.Enums.ApplicationStatus.Accepted));
        Assert.Equal(HttpStatusCode.OK, reviewResponse.StatusCode);
        var reviewed = await Data<MembershipApplicationDto>(reviewResponse);
        Assert.Equal(TeamUp.Domain.Enums.ApplicationStatus.Accepted, reviewed.Status);

        var membersResponse = await admin.GetAsync($"/api/v1/organizations/{organization.OrganizationId}/members");
        var members = await Data<List<OrganizationMemberDto>>(membersResponse);
        Assert.Contains(members, x => x.StudentId == applicantId && x.Role == "Member");
    }

    [Fact]
    public async Task PlatformAdmin_CanSoftDeleteOrganization()
    {
        using var admin = Client("PlatformAdmin", 9999);
        var request = new CreateOrganizationRequest($"Delete_{Guid.NewGuid():N}", "Description", "Other", null);
        var createResponse = await admin.PostAsJsonAsync("/api/v1/organizations", request);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await Data<OrganizationDetailDto>(createResponse);

        var deleteResponse = await admin.DeleteAsync($"/api/v1/organizations/{created.OrganizationId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getDeleted = await admin.GetAsync($"/api/v1/organizations/{created.OrganizationId}");
        Assert.Equal(HttpStatusCode.NotFound, getDeleted.StatusCode);
    }
}
