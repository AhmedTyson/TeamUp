using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using TeamUp.Application.Auth;
using TeamUp.Application.Projects;

namespace TeamUp.IntegrationTests;

public class ProjectsTeamsTasksTests(TeamUpApiFactory factory) : IClassFixture<TeamUpApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _anonymous = factory.CreateClient();

    [Fact]
    public async Task ProjectApplicationAndTaskLifecycle_WorksEndToEnd()
    {
        var creator = await RegisterClientAsync("creator");
        var applicant = await RegisterClientAsync("applicant");

        var createProject = await creator.PostAsJsonAsync("/api/v1/projects", new CreateProjectRequest(
            "TeamUp Test Project", "Integration test project", 2));
        Assert.Equal(HttpStatusCode.Created, createProject.StatusCode);
        var project = await createProject.Content.ReadFromJsonAsync<ProjectDetailDto>(JsonOptions);
        Assert.NotNull(project);
        Assert.Single(project.Members);
        Assert.Equal("Team Lead", project.Members[0].Role);

        var apply = await applicant.PostAsJsonAsync($"/api/v1/projects/{project.ProjectId}/applications",
            new CreateProjectApplicationRequest("I would like to contribute."));
        Assert.Equal(HttpStatusCode.Created, apply.StatusCode);
        var application = await apply.Content.ReadFromJsonAsync<ProjectApplicationDto>(JsonOptions);
        Assert.NotNull(application);

        var listApplications = await creator.GetAsync($"/api/v1/projects/{project.ProjectId}/applications");
        Assert.Equal(HttpStatusCode.OK, listApplications.StatusCode);

        var accept = await creator.PatchAsJsonAsync($"/api/v1/projects/{project.ProjectId}/applications/{application.ApplicationId}",
            new ReviewProjectApplicationRequest(TeamUp.Domain.Enums.ApplicationStatus.Accepted));
        Assert.Equal(HttpStatusCode.OK, accept.StatusCode);

        var members = await creator.GetFromJsonAsync<List<ProjectMemberDto>>(
            $"/api/v1/projects/{project.ProjectId}/members", JsonOptions);
        Assert.NotNull(members);
        Assert.Equal(2, members.Count);

        var createTask = await applicant.PostAsJsonAsync($"/api/v1/projects/{project.ProjectId}/tasks",
            new CreateProjectTaskRequest("Implement API", "Build the endpoint", DateTime.UtcNow.AddDays(2)));
        Assert.Equal(HttpStatusCode.Created, createTask.StatusCode);
        var task = await createTask.Content.ReadFromJsonAsync<ProjectTaskDto>(JsonOptions);
        Assert.NotNull(task);

        var assign = await creator.PutAsJsonAsync($"/api/v1/tasks/{task.TaskId}/assignee", new AssignTaskRequest(application.StudentId));
        Assert.Equal(HttpStatusCode.NoContent, assign.StatusCode);

        var taskDetails = await creator.GetFromJsonAsync<ProjectTaskDto>($"/api/v1/tasks/{task.TaskId}", JsonOptions);
        Assert.NotNull(taskDetails);
        Assert.Equal(application.StudentId, taskDetails.AssignedStudentId);

        var removeMember = await creator.DeleteAsync($"/api/v1/projects/{project.ProjectId}/members/{application.StudentId}");
        Assert.Equal(HttpStatusCode.NoContent, removeMember.StatusCode);

        var afterRemoval = await creator.GetFromJsonAsync<ProjectTaskDto>($"/api/v1/tasks/{task.TaskId}", JsonOptions);
        Assert.NotNull(afterRemoval);
        Assert.Null(afterRemoval.AssignedStudentId);

        var removedMemberTaskAccess = await applicant.GetAsync($"/api/v1/tasks/{task.TaskId}");
        Assert.Equal(HttpStatusCode.Unauthorized, removedMemberTaskAccess.StatusCode);

        var deleteTask = await creator.DeleteAsync($"/api/v1/tasks/{task.TaskId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteTask.StatusCode);
        var deletedTaskRead = await creator.GetAsync($"/api/v1/tasks/{task.TaskId}");
        Assert.Equal(HttpStatusCode.NotFound, deletedTaskRead.StatusCode);
    }

    [Fact]
    public async Task ProjectEndpoints_RequireAuthentication()
    {
        var response = await _anonymous.GetAsync("/api/v1/projects");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<HttpClient> RegisterClientAsync(string prefix)
    {
        var email = $"{prefix}_{Guid.NewGuid():N}@teamup.test";
        var request = new RegisterRequest(email, "P@ssword123!", prefix, "Tester", 2, factory.SeededFacultyId);
        var response = await _anonymous.PostAsJsonAsync("/api/v1/auth/register", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        return client;
    }
}
