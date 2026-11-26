using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TeamUp.Application.Common.Models;
using TeamUp.Application.Organizations;
using TeamUp.Application.Skills;
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
    public async Task OrganizationCategories_AreOutputCachedAndEvictedOnMutation()
    {
        var admin = CreateClientWithRole(998, "PlatformAdmin");

        var initialRes = await _client.GetAsync("/api/v1/organizations/public/categories");
        Assert.Equal(HttpStatusCode.OK, initialRes.StatusCode);
        var initialCategories = await GetDataAsync<List<string>>(initialRes);
        Assert.NotNull(initialCategories);

        var secondRes = await _client.GetAsync("/api/v1/organizations/public/categories");
        Assert.Equal(HttpStatusCode.OK, secondRes.StatusCode);

        var newCategory = $"Cat_{Guid.NewGuid():N}"[..12];
        var createOrgRes = await admin.PostAsJsonAsync(
            "/api/v1/organizations",
            new CreateOrganizationRequest(
                $"CacheOrg_{Guid.NewGuid():N}"[..16],
                "Testing output cache eviction for organization categories.",
                newCategory,
                null));
        Assert.Equal(HttpStatusCode.Created, createOrgRes.StatusCode);

        var refreshedRes = await _client.GetAsync("/api/v1/organizations/public/categories");
        Assert.Equal(HttpStatusCode.OK, refreshedRes.StatusCode);
        var refreshedCategories = await GetDataAsync<List<string>>(refreshedRes);
        Assert.Contains(newCategory, refreshedCategories);
    }

    [Fact]
    public async Task SkillsCatalog_IsOutputCachedAndEvictedOnMutation()
    {
        var admin = CreateClientWithRole(998, "PlatformAdmin");

        var initialRes = await admin.GetAsync("/api/v1/skills");
        Assert.Equal(HttpStatusCode.OK, initialRes.StatusCode);
        var initialSkills = await GetDataAsync<List<SkillDto>>(initialRes);
        Assert.NotNull(initialSkills);

        var secondRes = await admin.GetAsync("/api/v1/skills");
        Assert.Equal(HttpStatusCode.OK, secondRes.StatusCode);

        var newSkillName = $"Skill_{Guid.NewGuid():N}"[..16];
        var createSkillRes = await admin.PostAsJsonAsync(
            "/api/v1/skills",
            new CreateSkillRequest(newSkillName, "Technical", "OutputCache eviction test skill"));
        Assert.Equal(HttpStatusCode.Created, createSkillRes.StatusCode);

        var refreshedRes = await admin.GetAsync("/api/v1/skills");
        Assert.Equal(HttpStatusCode.OK, refreshedRes.StatusCode);
        var refreshedSkills = await GetDataAsync<List<SkillDto>>(refreshedRes);
        Assert.Contains(refreshedSkills, s => s.Name == newSkillName);
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

    [Fact]
    public async Task AdminRestore_WithPluralEntityType_EvictsSkillsOutputCache()
    {
        var admin = CreateClientWithRole(998, "PlatformAdmin");
        var skillName = $"PluralRestore_{Guid.NewGuid():N}"[..20];

        var createRes = await admin.PostAsJsonAsync(
            "/api/v1/skills",
            new CreateSkillRequest(skillName, "Technical", "Plural restore cache eviction check"));
        Assert.Equal(HttpStatusCode.Created, createRes.StatusCode);
        var created = await GetDataAsync<SkillDto>(createRes);

        var deleteRes = await admin.DeleteAsync($"/api/v1/skills/{created.SkillId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteRes.StatusCode);

        // Warm OutputCache while the skill is soft-deleted.
        var cachedBeforeRestore = await admin.GetAsync("/api/v1/skills");
        Assert.Equal(HttpStatusCode.OK, cachedBeforeRestore.StatusCode);
        var skillsBeforeRestore = await GetDataAsync<List<SkillDto>>(cachedBeforeRestore);
        Assert.DoesNotContain(skillsBeforeRestore, s => s.SkillId == created.SkillId);

        // Restore using plural entityType route.
        var restoreRes = await admin.PostAsync($"/api/v1/admin/restore/skills/{created.SkillId}", null);
        Assert.Equal(HttpStatusCode.OK, restoreRes.StatusCode);

        var afterRestoreRes = await admin.GetAsync("/api/v1/skills");
        Assert.Equal(HttpStatusCode.OK, afterRestoreRes.StatusCode);
        var skillsAfterRestore = await GetDataAsync<List<SkillDto>>(afterRestoreRes);
        Assert.Contains(skillsAfterRestore, s => s.SkillId == created.SkillId);
    }
}
