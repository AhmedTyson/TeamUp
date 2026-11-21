using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TeamUp.Application.Admin;
using TeamUp.Application.Auth;
using TeamUp.Application.Common.Models;
using TeamUp.Application.Universities;
using TeamUp.Domain.Enums;

namespace TeamUp.IntegrationTests;

public class AdminTests(TeamUpApiFactory factory) : IClassFixture<TeamUpApiFactory>
{
    private HttpClient CreateClientWithRole(string role)
    {
        var client = factory.CreateClient();
        var token = factory.CreateToken(900, $"{role.ToLower()}@teamup.test", role);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static async Task<T> GetDataAsync<T>(HttpResponseMessage res)
    {
        var envelope = await res.Content.ReadFromJsonAsync<BaseResponse<T>>();
        Assert.NotNull(envelope);
        Assert.True(envelope.Success);
        Assert.NotNull(envelope.Data);
        return envelope.Data;
    }

    [Fact]
    public async Task AdminDashboard_AsStudent_Returns403Forbidden()
    {
        using var client = CreateClientWithRole("Student");
        var res = await client.GetAsync("/api/v1/admin/dashboard");
        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
    }

    [Fact]
    public async Task AdminDashboard_AsAdmin_Returns200WithStats()
    {
        using var client = CreateClientWithRole("PlatformAdmin");
        var res = await client.GetAsync("/api/v1/admin/dashboard");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var stats = await GetDataAsync<AdminDashboardDto>(res);
        Assert.NotNull(stats);
        Assert.True(stats.ActiveUniversitiesCount >= 1);
        Assert.True(stats.ActiveFacultiesCount >= 1);
        Assert.NotNull(stats.RoleDistribution);
    }

    [Fact]
    public async Task UpdateStudentRole_AsAdmin_PromotesUser()
    {
        using var publicClient = factory.CreateClient();
        var email = $"promote_{Guid.NewGuid():N}@test.com";
        var reg = new RegisterRequest("Tariq", "Lead", email, "Password123!", 2, factory.SeededFacultyId);
        var regRes = await publicClient.PostAsJsonAsync("/api/v1/auth/register", reg);
        var auth = await GetDataAsync<AuthResponse>(regRes);

        using var adminClient = CreateClientWithRole("PlatformAdmin");
        var promoteReq = new UpdateUserRoleRequest(UserRole.OrgAdmin);
        var promoteRes = await adminClient.PatchAsJsonAsync($"/api/v1/admin/students/{auth.Student.StudentId}/role", promoteReq);
        Assert.Equal(HttpStatusCode.OK, promoteRes.StatusCode);

        var updated = await GetDataAsync<StudentSummaryDto>(promoteRes);
        Assert.Equal("OrgAdmin", updated.Role);
    }

    [Fact]
    public async Task SoftDelete_AndRestore_Lifecycle_WorksEndToEnd()
    {
        using var adminClient = CreateClientWithRole("PlatformAdmin");

        // 1. Create a university to test restore
        var uniName = $"Restore_Uni_{Guid.NewGuid():N}";
        var createRes = await adminClient.PostAsJsonAsync("/api/v1/universities", new CreateUniversityRequest(uniName, "Suez", "Egypt", null));
        Assert.Equal(HttpStatusCode.Created, createRes.StatusCode);
        var uni = await GetDataAsync<UniversityDetailDto>(createRes);

        // 2. Soft-delete the university
        var delRes = await adminClient.DeleteAsync($"/api/v1/universities/{uni.UniversityId}");
        Assert.Equal(HttpStatusCode.NoContent, delRes.StatusCode);

        // 3. Verify it appears in soft-deleted records list
        var softDelRes = await adminClient.GetAsync("/api/v1/admin/soft-deleted?entityType=university");
        Assert.Equal(HttpStatusCode.OK, softDelRes.StatusCode);
        var softDeleted = await GetDataAsync<List<SoftDeletedItemDto>>(softDelRes);
        Assert.Contains(softDeleted, item => item.Id == uni.UniversityId && item.EntityType == "University");

        // 4. Restore the university
        var restoreRes = await adminClient.PostAsync($"/api/v1/admin/restore/university/{uni.UniversityId}", null);
        Assert.Equal(HttpStatusCode.OK, restoreRes.StatusCode);

        // 5. Verify university is active again
        var getRestoredRes = await adminClient.GetAsync($"/api/v1/universities/{uni.UniversityId}");
        Assert.Equal(HttpStatusCode.OK, getRestoredRes.StatusCode);
        var restoredUni = await GetDataAsync<UniversityDetailDto>(getRestoredRes);
        Assert.Equal(uniName, restoredUni.Name);
    }

    [Fact]
    public async Task RestoreFaculty_WhenParentUniversityDeleted_Returns409Conflict()
    {
        using var adminClient = CreateClientWithRole("PlatformAdmin");

        // 1. Create university + faculty
        var uniName = $"Cascade_Uni_{Guid.NewGuid():N}";
        var createUniRes = await adminClient.PostAsJsonAsync("/api/v1/universities", new CreateUniversityRequest(uniName, "Tanta", "Egypt", null));
        var uni = await GetDataAsync<UniversityDetailDto>(createUniRes);

        var facRes = await adminClient.PostAsJsonAsync($"/api/v1/universities/{uni.UniversityId}/faculties", new CreateFacultyRequest("Faculty of Pharmacy", "PHARM"));
        var fac = await GetDataAsync<FacultyDto>(facRes);

        // 2. Soft-delete university (which cascades soft-delete to faculty)
        await adminClient.DeleteAsync($"/api/v1/universities/{uni.UniversityId}");

        // 3. Attempting to restore faculty alone while university is deleted must fail with 409 Conflict
        var restoreFacRes = await adminClient.PostAsync($"/api/v1/admin/restore/faculty/{fac.FacultyId}", null);
        Assert.Equal(HttpStatusCode.Conflict, restoreFacRes.StatusCode);
    }
}
