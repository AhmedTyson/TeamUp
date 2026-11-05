using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using TeamUp.Application.Auth;
using TeamUp.Application.Common.Models;
using TeamUp.Application.Skills;
using TeamUp.Domain.Enums;

namespace TeamUp.IntegrationTests;

public class SkillsTests(TeamUpApiFactory factory) : IClassFixture<TeamUpApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _anonymous = factory.CreateClient();

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
    public async Task MasterSkillsAndStudentClaims_FullLifecycle_WorksEndToEnd()
    {
        var admin = CreateClientWithRole(901, "PlatformAdmin");

        // 1. Register a real student in DB
        var email = $"student_skill_{Guid.NewGuid():N}@teamup.test";
        var reg = new RegisterRequest("Skill", "Tester", email, "P@ssword123!", 3, factory.SeededFacultyId);
        var regRes = await _anonymous.PostAsJsonAsync("/api/v1/auth/register", reg);
        Assert.Equal(HttpStatusCode.Created, regRes.StatusCode);
        var auth = await GetDataAsync<AuthResponse>(regRes);

        var student = factory.CreateClient();
        student.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // 2. Regular student cannot create master skill (403 Forbidden)
        var forbiddenRes = await student.PostAsJsonAsync("/api/v1/skills", new CreateSkillRequest("Rust", "Systems", "Rust programming"));
        Assert.Equal(HttpStatusCode.Forbidden, forbiddenRes.StatusCode);

        // 3. PlatformAdmin creates master skill (201 Created)
        var uniqueSkillName = $"TypeScript_{Guid.NewGuid():N}";
        var createRes = await admin.PostAsJsonAsync("/api/v1/skills", new CreateSkillRequest(uniqueSkillName, "Frontend", "TypeScript language"));
        Assert.Equal(HttpStatusCode.Created, createRes.StatusCode);
        var createdSkill = await GetDataAsync<SkillDto>(createRes);
        Assert.Equal(uniqueSkillName, createdSkill.Name);

        // 4. List skills and filter
        var listRes = await student.GetAsync($"/api/v1/skills?search={uniqueSkillName}");
        Assert.Equal(HttpStatusCode.OK, listRes.StatusCode);
        var skills = await GetDataAsync<List<SkillDto>>(listRes);
        Assert.Contains(skills, s => s.SkillId == createdSkill.SkillId);

        // 5. Student claims skill with proficiency and years
        var claimRes = await student.PutAsJsonAsync($"/api/v1/students/me/skills/{createdSkill.SkillId}",
            new ClaimSkillRequest(ProficiencyLevel.Intermediate, 2.5m));
        Assert.Equal(HttpStatusCode.OK, claimRes.StatusCode);
        var claimed = await GetDataAsync<StudentSkillDto>(claimRes);
        Assert.Equal(ProficiencyLevel.Intermediate, claimed.ProficiencyLevel);
        Assert.Equal(2.5m, claimed.YearsOfExperience);

        // 6. Get student claimed skills
        var studentId = auth.Student.StudentId;
        var getStudentSkillsRes = await admin.GetAsync($"/api/v1/students/{studentId}/skills");
        Assert.Equal(HttpStatusCode.OK, getStudentSkillsRes.StatusCode);
        var studentSkills = await GetDataAsync<List<StudentSkillDto>>(getStudentSkillsRes);
        Assert.Contains(studentSkills, s => s.SkillId == createdSkill.SkillId);

        // 7. Remove claimed skill
        var removeClaimRes = await student.DeleteAsync($"/api/v1/students/me/skills/{createdSkill.SkillId}");
        Assert.Equal(HttpStatusCode.NoContent, removeClaimRes.StatusCode);

        // 8. Verify claimed skill is removed
        var afterRemovalRes = await student.GetAsync($"/api/v1/students/{studentId}/skills");
        var afterRemoval = await GetDataAsync<List<StudentSkillDto>>(afterRemovalRes);
        Assert.DoesNotContain(afterRemoval, s => s.SkillId == createdSkill.SkillId);

        // 9. Admin soft-deletes master skill
        var deleteSkillRes = await admin.DeleteAsync($"/api/v1/skills/{createdSkill.SkillId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteSkillRes.StatusCode);

        // 10. Subsequent GET returns 404
        var deletedRead = await student.GetAsync($"/api/v1/skills/{createdSkill.SkillId}");
        Assert.Equal(HttpStatusCode.NotFound, deletedRead.StatusCode);
    }
}
