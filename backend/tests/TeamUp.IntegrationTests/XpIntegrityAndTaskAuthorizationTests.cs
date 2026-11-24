// Date: 2026-11-24
// Purpose: Permanent regression tests for TU-SEC-001 (XP idempotency incl. concurrency) and TU-SEC-002 (task status authorization).

using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TeamUp.Application.Projects;
using TeamUp.Domain.Enums;
using TeamUp.Infrastructure.Persistence;

namespace TeamUp.IntegrationTests;

public class XpIntegrityAndTaskAuthorizationTests(TeamUpApiFactory factory) : IClassFixture<TeamUpApiFactory>
{
    private readonly ApiHelper _api = new(factory);

    private async Task<(HttpClient Client, int Id, ProjectTaskDto Task)> AssignedTaskAsync(string prefix)
    {
        var (client, id, _, _) = await _api.RegisterAsync(prefix);
        var project = await _api.CreateProjectAsync(client);
        var task = await _api.CreateTaskAsync(client, project.ProjectId, "Original Title");
        await ApiHelper.AssignAsync(client, task.TaskId, id);
        return (client, id, task);
    }

    [Fact]
    public async Task RenamingAndRecompletingTask_DoesNotPayXpTwice()
    {
        var (client, id, task) = await AssignedTaskAsync("xpedit");
        Assert.Equal(HttpStatusCode.OK, (await ApiHelper.SetStatusAsync(client, task.TaskId, ProjectTaskStatus.Done)).StatusCode);
        Assert.Equal(50, await ApiHelper.ScoreAsync(client, id));

        await client.PatchAsJsonAsync($"/api/v1/tasks/{task.TaskId}",
            new UpdateProjectTaskRequest("Renamed", null, null, null, null), ApiHelper.Json);
        await ApiHelper.SetStatusAsync(client, task.TaskId, ProjectTaskStatus.Todo);
        Assert.Equal(HttpStatusCode.OK, (await ApiHelper.SetStatusAsync(client, task.TaskId, ProjectTaskStatus.Done)).StatusCode);

        Assert.Equal(50, await ApiHelper.ScoreAsync(client, id));
    }

    [Fact]
    public async Task ReassigningACompletedTask_DoesNotPaySecondAssignee()
    {
        var (creator, creatorId, task) = await AssignedTaskAsync("reassign");
        await ApiHelper.SetStatusAsync(creator, task.TaskId, ProjectTaskStatus.Done);

        var (other, otherId, _, _) = await _api.RegisterAsync("reassignee");
        // Make 'other' a project member directly, then reassign and complete again.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var projectId = (await db.ProjectTasks.SingleAsync(t => t.TaskId == task.TaskId)).ProjectId;
            db.ProjectMembers.Add(new TeamUp.Domain.Entities.ProjectMember { ProjectId = projectId, StudentId = otherId, Role = "Member" });
            await db.SaveChangesAsync();
        }
        await ApiHelper.AssignAsync(creator, task.TaskId, otherId);
        await ApiHelper.SetStatusAsync(creator, task.TaskId, ProjectTaskStatus.Todo);
        await ApiHelper.SetStatusAsync(creator, task.TaskId, ProjectTaskStatus.Done);

        Assert.Equal(50, await ApiHelper.ScoreAsync(creator, creatorId));
        Assert.Equal(0, await ApiHelper.ScoreAsync(other, otherId));
    }

    [Fact]
    public async Task NonAssigneeMember_CannotChangeStatus_AndNoXpMoves()
    {
        var (creator, creatorId, task) = await AssignedTaskAsync("auth");
        var (member, memberId, _, _) = await _api.RegisterAsync("bystander");
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var projectId = (await db.ProjectTasks.SingleAsync(t => t.TaskId == task.TaskId)).ProjectId;
            db.ProjectMembers.Add(new TeamUp.Domain.Entities.ProjectMember { ProjectId = projectId, StudentId = memberId, Role = "Member" });
            await db.SaveChangesAsync();
        }

        var res = await ApiHelper.SetStatusAsync(member, task.TaskId, ProjectTaskStatus.Done);
        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
        Assert.Equal(0, await ApiHelper.ScoreAsync(creator, creatorId));

        // Non-members stay out (403, not 401, so the SPA does not mistake it for an expired session).
        var (outsider, _, _, _) = await _api.RegisterAsync("outsider");
        Assert.Equal(HttpStatusCode.Forbidden, (await ApiHelper.SetStatusAsync(outsider, task.TaskId, ProjectTaskStatus.Done)).StatusCode);
    }

    [Fact]
    public async Task Assignee_CanComplete_AndMembersCanStillEditTitle()
    {
        var (creator, creatorId, task) = await AssignedTaskAsync("ok");
        var rename = await creator.PatchAsJsonAsync($"/api/v1/tasks/{task.TaskId}",
            new UpdateProjectTaskRequest("New title", null, null, null, null), ApiHelper.Json);
        Assert.Equal(HttpStatusCode.OK, rename.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await ApiHelper.SetStatusAsync(creator, task.TaskId, ProjectTaskStatus.Done)).StatusCode);
        Assert.Equal(50, await ApiHelper.ScoreAsync(creator, creatorId));
    }

    [Fact]
    public async Task DatabaseRejectsDuplicateAwardKey_ButAllowsRepeatedManualAdjustments()
    {
        var (_, id, _, _) = await _api.RegisterAsync("dbkey");
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        db.ScoreTransactions.Add(new TeamUp.Domain.Entities.ScoreTransaction
            { StudentId = id, Points = 10, Reason = "m1", SourceType = ScoreSourceType.Manual });
        db.ScoreTransactions.Add(new TeamUp.Domain.Entities.ScoreTransaction
            { StudentId = id, Points = 10, Reason = "m2", SourceType = ScoreSourceType.Manual });
        await db.SaveChangesAsync();

        db.ScoreTransactions.Add(new TeamUp.Domain.Entities.ScoreTransaction
            { StudentId = id, Points = 50, Reason = "a", SourceType = ScoreSourceType.TaskCompleted, SourceReferenceId = 777 });
        await db.SaveChangesAsync();
        db.ScoreTransactions.Add(new TeamUp.Domain.Entities.ScoreTransaction
            { StudentId = id, Points = 50, Reason = "b", SourceType = ScoreSourceType.TaskCompleted, SourceReferenceId = 777 });
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }
}

public class XpConcurrencyTests(FileDbApiFactory factory) : IClassFixture<FileDbApiFactory>
{
    private readonly ApiHelper _api = new(factory);

    [Fact]
    public async Task ParallelCompletions_PayXpExactlyOnce_AndNeverReturnServerError()
    {
        for (var run = 0; run < 5; run++)
        {
            var (client, id, task) = await AssignedAsync($"race{run}");
            var responses = await Task.WhenAll(Enumerable.Range(0, 6)
                .Select(_ => ApiHelper.SetStatusAsync(client, task.TaskId, ProjectTaskStatus.Done)));

            Assert.All(responses, r => Assert.True((int)r.StatusCode < 500, $"unexpected {(int)r.StatusCode}: {r.Content.ReadAsStringAsync().Result}"));
            Assert.Equal(50, await ApiHelper.ScoreAsync(client, id));
        }
    }

    private async Task<(HttpClient Client, int Id, ProjectTaskDto Task)> AssignedAsync(string prefix)
    {
        var (client, id, _, _) = await _api.RegisterAsync(prefix);
        var project = await _api.CreateProjectAsync(client);
        var task = await _api.CreateTaskAsync(client, project.ProjectId, "Race Task");
        await ApiHelper.AssignAsync(client, task.TaskId, id);
        return (client, id, task);
    }
}
