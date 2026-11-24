// Date: 2026-11-24
// Purpose: Shared helpers for security-regression integration tests (register users, call the API, read the response envelope).

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using TeamUp.Application.Auth;
using TeamUp.Application.Common.Models;
using TeamUp.Application.Gamification;
using TeamUp.Application.Projects;
using TeamUp.Domain.Enums;

namespace TeamUp.IntegrationTests;

public sealed class ApiHelper(TeamUpApiFactory factory)
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public TeamUpApiFactory Factory { get; } = factory;

    public static async Task<T> DataAsync<T>(HttpResponseMessage res)
    {
        var envelope = await res.Content.ReadFromJsonAsync<BaseResponse<T>>(Json);
        Assert.NotNull(envelope);
        Assert.True(envelope.Success);
        Assert.NotNull(envelope.Data);
        return envelope.Data;
    }

    public async Task<(HttpClient Client, int StudentId, string Email, AuthResponse Auth)> RegisterAsync(string prefix, string password = "P@ssword123!")
    {
        var email = $"{prefix}_{Guid.NewGuid():N}@teamup.test";
        var anonymous = Factory.CreateClient();
        var res = await anonymous.PostAsJsonAsync("/api/v1/auth/register",
            new RegisterRequest(prefix, "Tester", email, password, 2, Factory.SeededFacultyId), Json);
        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
        var auth = await DataAsync<AuthResponse>(res);
        return (Authed(auth.AccessToken), auth.Student.StudentId, email, auth);
    }

    public HttpClient Authed(string accessToken)
    {
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }

    public async Task<ProjectDetailDto> CreateProjectAsync(HttpClient client, int capacity = 5)
    {
        var res = await client.PostAsJsonAsync("/api/v1/projects",
            new CreateProjectRequest($"P {Guid.NewGuid():N}", "regression project", capacity), Json);
        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
        return await DataAsync<ProjectDetailDto>(res);
    }

    public async Task<ProjectTaskDto> CreateTaskAsync(HttpClient client, int projectId, string title)
    {
        var res = await client.PostAsJsonAsync($"/api/v1/projects/{projectId}/tasks",
            new CreateProjectTaskRequest(title, null, null), Json);
        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
        return await DataAsync<ProjectTaskDto>(res);
    }

    public static async Task AssignAsync(HttpClient client, int taskId, int studentId)
    {
        var res = await client.PutAsJsonAsync($"/api/v1/tasks/{taskId}/assignee", new AssignTaskRequest(studentId), Json);
        Assert.Equal(HttpStatusCode.NoContent, res.StatusCode);
    }

    public static Task<HttpResponseMessage> SetStatusAsync(HttpClient client, int taskId, ProjectTaskStatus status) =>
        client.PatchAsJsonAsync($"/api/v1/tasks/{taskId}", new UpdateProjectTaskRequest(null, null, status, null, null), Json);

    public static async Task<int> ScoreAsync(HttpClient client, int studentId)
    {
        var res = await client.GetAsync($"/api/v1/students/{studentId}/score");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        return (await DataAsync<StudentScoreDto>(res)).TotalPoints;
    }
}
