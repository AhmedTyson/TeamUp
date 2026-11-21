using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TeamUp.Application.Common.Models;
using TeamUp.Application.Universities;
using TeamUp.Domain.Entities;
using TeamUp.Infrastructure.Persistence;

namespace TeamUp.IntegrationTests;

public class CachingAndConcurrencyTests(TeamUpApiFactory factory) : IClassFixture<TeamUpApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _client = factory.CreateClient();

    private HttpClient CreateClientWithRole(int studentId, string role)
    {
        var client = factory.CreateClient();
        var token = factory.CreateToken(studentId, $"user_{studentId}@teamup.test", role);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static async Task<T> GetDataAsync<T>(HttpResponseMessage res)
    {
        var envelope = await res.Content.ReadFromJsonAsync<BaseResponse<T>>(JsonOptions);
        Assert.NotNull(envelope);
        Assert.True(envelope.Success);
        Assert.NotNull(envelope.Data);
        return envelope.Data;
    }

    [Fact]
    public async Task AcademicCatalog_IsCachedAndInvalidatedOnMutation()
    {
        var admin = CreateClientWithRole(998, "PlatformAdmin");

        // 1. Initial call to catalog populates cache
        var res1 = await _client.GetAsync("/api/v1/universities/catalog");
        Assert.Equal(HttpStatusCode.OK, res1.StatusCode);
        var catalog1 = await GetDataAsync<List<UniversityDetailDto>>(res1);
        Assert.NotEmpty(catalog1);

        // 2. Call again - served from cache
        var res2 = await _client.GetAsync("/api/v1/universities/catalog");
        Assert.Equal(HttpStatusCode.OK, res2.StatusCode);
        var catalog2 = await GetDataAsync<List<UniversityDetailDto>>(res2);
        Assert.Equal(catalog1.Count, catalog2.Count);

        // 3. Admin creates a new faculty under seeded university - invalidates catalog cache
        var facultyCode = $"MED_{Guid.NewGuid():N}"[..8].ToUpperInvariant();
        var createFacultyRes = await admin.PostAsJsonAsync(
            $"/api/v1/universities/{factory.SeededUniversityId}/faculties",
            new CreateFacultyRequest($"Faculty of Medicine {Guid.NewGuid():N}", facultyCode));
        Assert.Equal(HttpStatusCode.Created, createFacultyRes.StatusCode);

        // 4. Subsequent catalog call fetches freshly updated catalog including the new faculty
        var res3 = await _client.GetAsync("/api/v1/universities/catalog");
        Assert.Equal(HttpStatusCode.OK, res3.StatusCode);
        var catalog3 = await GetDataAsync<List<UniversityDetailDto>>(res3);
        var uni = catalog3.First(u => u.UniversityId == factory.SeededUniversityId);
        Assert.Contains(uni.Faculties, f => f.Code == facultyCode);
    }

    [Fact]
    public async Task ProjectAndEvent_RowVersion_UpdatesOnModification()
    {
        int projectId;
        byte[] initialRowVersion;

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var student = new Student
            {
                Email = $"concur_{Guid.NewGuid():N}@teamup.test",
                PasswordHash = "hash",
                FirstName = "Conc",
                LastName = "User",
                AcademicYear = 2,
                FacultyId = factory.SeededFacultyId
            };
            db.Students.Add(student);
            await db.SaveChangesAsync();

            var project = new Project
            {
                Title = $"Concurrency Project {Guid.NewGuid():N}",
                Description = "Testing RowVersion tokens",
                MaxTeamSize = 4,
                CreatorId = student.StudentId,
            };
            db.Projects.Add(project);
            await db.SaveChangesAsync();

            projectId = project.ProjectId;
            initialRowVersion = project.RowVersion;
            Assert.NotEmpty(initialRowVersion);
        }

        // Modify project
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var project = await db.Projects.FirstAsync(p => p.ProjectId == projectId);
            project.Title = "Updated Concurrency Title";
            await db.SaveChangesAsync();

            Assert.NotEmpty(project.RowVersion);
            Assert.NotEqual(initialRowVersion, project.RowVersion); // RowVersion changed on update
        }
    }
}
