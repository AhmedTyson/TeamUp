using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TeamUp.Application.Common;
using TeamUp.Application.Common.Models;
using TeamUp.Application.Universities;

namespace TeamUp.IntegrationTests;

public class UniversityAndFacultyTests(TeamUpApiFactory factory) : IClassFixture<TeamUpApiFactory>
{
    private HttpClient CreateClientWithRole(string role)
    {
        var client = factory.CreateClient();
        var token = factory.CreateToken(100, "user@test.com", role);
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
    public async Task GetUniversities_Unauthenticated_Returns401()
    {
        using var client = factory.CreateClient();
        var res = await client.GetAsync("/api/v1/universities");
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task GetUniversities_Authenticated_ReturnsPagedResult()
    {
        using var client = CreateClientWithRole("Student");
        var res = await client.GetAsync("/api/v1/universities");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var paged = await GetDataAsync<PagedResult<UniversityDto>>(res);
        Assert.NotNull(paged);
        Assert.NotEmpty(paged.Items);
    }

    [Fact]
    public async Task GetUniversityById_Existing_ReturnsDetailsWithFaculties()
    {
        using var client = CreateClientWithRole("Student");
        var res = await client.GetAsync($"/api/v1/universities/{factory.SeededUniversityId}");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var uni = await GetDataAsync<UniversityDetailDto>(res);
        Assert.Equal("Cairo University Test", uni.Name);
        Assert.NotEmpty(uni.Faculties);
    }

    [Fact]
    public async Task GetUniversityById_NonExisting_Returns404()
    {
        using var client = CreateClientWithRole("Student");
        var res = await client.GetAsync("/api/v1/universities/999999");
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task CreateUniversity_AsStudent_Returns403Forbidden()
    {
        using var client = CreateClientWithRole("Student");
        var req = new CreateUniversityRequest("Forbidden Uni", "Alexandria", "Egypt", null);
        var res = await client.PostAsJsonAsync("/api/v1/universities", req);
        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
    }

    [Fact]
    public async Task University_FullLifecycle_AsAdmin_WorksAsExpected()
    {
        using var client = CreateClientWithRole("PlatformAdmin");

        // 1. Create University
        var name = $"Uni_{Guid.NewGuid():N}";
        var req = new CreateUniversityRequest(name, "Giza", "Egypt", "https://newuni.edu.eg");
        var createRes = await client.PostAsJsonAsync("/api/v1/universities", req);
        Assert.Equal(HttpStatusCode.Created, createRes.StatusCode);

        var created = await GetDataAsync<UniversityDetailDto>(createRes);
        Assert.Equal(name, created.Name);

        // 2. Duplicate Name returns 409 Conflict
        var dupRes = await client.PostAsJsonAsync("/api/v1/universities", req);
        Assert.Equal(HttpStatusCode.Conflict, dupRes.StatusCode);

        // 3. Update University
        var updateReq = new UpdateUniversityRequest($"{name} Updated", "Cairo", "Egypt", "https://updated.edu.eg");
        var updateRes = await client.PutAsJsonAsync($"/api/v1/universities/{created.UniversityId}", updateReq);
        Assert.Equal(HttpStatusCode.OK, updateRes.StatusCode);

        var updated = await GetDataAsync<UniversityDetailDto>(updateRes);
        Assert.Equal($"{name} Updated", updated.Name);

        // 4. Create child faculty
        var facReq = new CreateFacultyRequest("Faculty of Engineering", "ENG_NEW");
        var createFacRes = await client.PostAsJsonAsync($"/api/v1/universities/{created.UniversityId}/faculties", facReq);
        Assert.Equal(HttpStatusCode.Created, createFacRes.StatusCode);
        var createdFac = await GetDataAsync<FacultyDto>(createFacRes);
        Assert.Equal("ENG_NEW", createdFac.Code);

        // 5. Duplicate faculty code in same university returns 409
        var dupFacRes = await client.PostAsJsonAsync($"/api/v1/universities/{created.UniversityId}/faculties", facReq);
        Assert.Equal(HttpStatusCode.Conflict, dupFacRes.StatusCode);

        // 6. Update faculty
        var updateFacReq = new UpdateFacultyRequest("Faculty of Engineering & Tech", "ENG_UPDATED");
        var updateFacRes = await client.PutAsJsonAsync($"/api/v1/faculties/{createdFac.FacultyId}", updateFacReq);
        Assert.Equal(HttpStatusCode.OK, updateFacRes.StatusCode);
        var updatedFac = await GetDataAsync<FacultyDto>(updateFacRes);
        Assert.Equal("ENG_UPDATED", updatedFac.Code);

        // 7. Get faculties list for university
        var listFacRes = await client.GetAsync($"/api/v1/universities/{created.UniversityId}/faculties");
        Assert.Equal(HttpStatusCode.OK, listFacRes.StatusCode);
        var faculties = await GetDataAsync<List<FacultyDto>>(listFacRes);
        Assert.Single(faculties);

        // 8. Soft-delete faculty
        var delFacRes = await client.DeleteAsync($"/api/v1/faculties/{createdFac.FacultyId}");
        Assert.Equal(HttpStatusCode.NoContent, delFacRes.StatusCode);

        // 9. Soft-delete university
        var delUniRes = await client.DeleteAsync($"/api/v1/universities/{created.UniversityId}");
        Assert.Equal(HttpStatusCode.NoContent, delUniRes.StatusCode);

        // 10. Querying deleted university returns 404
        var getDeletedRes = await client.GetAsync($"/api/v1/universities/{created.UniversityId}");
        Assert.Equal(HttpStatusCode.NotFound, getDeletedRes.StatusCode);
    }
}
