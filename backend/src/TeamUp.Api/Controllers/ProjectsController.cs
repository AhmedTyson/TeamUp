using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Api.Controllers.Common;
using TeamUp.Application.Common;
using TeamUp.Application.Projects;

namespace TeamUp.Api.Controllers;

[Route("api/v1/projects")]
[Authorize]
public class ProjectsController(
    IProjectService projectService,
    IProjectSkillService skillService,
    IProjectMemberService memberService,
    IProjectTaskService taskService) : BaseApiController
{
    [HttpGet]
    [ProducesOk<PagedResult<ProjectListItemDto>>]
    public async Task<IActionResult> GetProjects(
        [FromQuery] string? keyword,
        [FromQuery] string? status,
        [FromQuery] int? skillId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await projectService.GetProjectsAsync(keyword, status, skillId, new PageRequest(page, pageSize), ct);
        return OkResponse(result);
    }

    [HttpPost]
    [ProducesCreated<ProjectDetailDto>]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request, CancellationToken ct)
    {
        var result = await projectService.CreateProjectAsync(CurrentStudentId, request, ct);
        return CreatedResponse(nameof(GetProject), new { id = result.ProjectId }, result, "Project created successfully.");
    }

    [HttpGet("{id:int}")]
    [ProducesOk<ProjectDetailDto>, ProducesNotFound]
    public async Task<IActionResult> GetProject([FromRoute] int id, CancellationToken ct)
    {
        var result = await projectService.GetProjectAsync(id, ct);
        return OkResponse(result);
    }

    [HttpPatch("{id:int}")]
    [ProducesOk<ProjectDetailDto>, ProducesNotFound]
    public async Task<IActionResult> UpdateProject([FromRoute] int id, [FromBody] UpdateProjectRequest request, CancellationToken ct)
    {
        var result = await projectService.UpdateProjectAsync(CurrentStudentId, id, request, ct);
        return OkResponse(result, "Project updated successfully.");
    }

    [HttpDelete("{id:int}")]
    [ProducesNoContent, ProducesNotFound]
    public async Task<IActionResult> DeleteProject([FromRoute] int id, CancellationToken ct)
    {
        await projectService.DeleteProjectAsync(CurrentStudentId, id, ct);
        return NoContentResponse();
    }

    [HttpGet("{id:int}/required-skills")]
    [ProducesOk<IReadOnlyList<ProjectSkillDto>>, ProducesNotFound]
    public async Task<IActionResult> GetRequiredSkills([FromRoute] int id, CancellationToken ct)
    {
        var result = await skillService.GetRequiredSkillsAsync(id, ct);
        return OkResponse(result);
    }

    [HttpPut("{id:int}/required-skills/{skillId:int}")]
    [ProducesNoContent, ProducesNotFound]
    public async Task<IActionResult> UpsertRequiredSkill(
        [FromRoute] int id,
        [FromRoute] int skillId,
        [FromBody] UpsertRequiredSkillRequest request,
        CancellationToken ct)
    {
        await skillService.UpsertRequiredSkillAsync(CurrentStudentId, id, skillId, request, ct);
        return NoContentResponse();
    }

    [HttpDelete("{id:int}/required-skills/{skillId:int}")]
    [ProducesNoContent, ProducesNotFound]
    public async Task<IActionResult> RemoveRequiredSkill([FromRoute] int id, [FromRoute] int skillId, CancellationToken ct)
    {
        await skillService.RemoveRequiredSkillAsync(CurrentStudentId, id, skillId, ct);
        return NoContentResponse();
    }

    [HttpGet("{id:int}/members")]
    [ProducesOk<IReadOnlyList<ProjectMemberDto>>, ProducesNotFound]
    public async Task<IActionResult> GetMembers([FromRoute] int id, CancellationToken ct)
    {
        var result = await memberService.GetMembersAsync(id, ct);
        return OkResponse(result);
    }

    [HttpPatch("{id:int}/members/{studentId:int}")]
    [ProducesNoContent, ProducesNotFound]
    public async Task<IActionResult> UpdateMemberRole(
        [FromRoute] int id,
        [FromRoute] int studentId,
        [FromBody] UpdateProjectMemberRequest request,
        CancellationToken ct)
    {
        await memberService.UpdateMemberRoleAsync(CurrentStudentId, id, studentId, request, ct);
        return NoContentResponse();
    }

    [HttpDelete("{id:int}/members/{studentId:int}")]
    [ProducesNoContent, ProducesNotFound]
    public async Task<IActionResult> RemoveMember([FromRoute] int id, [FromRoute] int studentId, CancellationToken ct)
    {
        await memberService.RemoveMemberAsync(CurrentStudentId, id, studentId, ct);
        return NoContentResponse();
    }

    [HttpGet("{id:int}/applications")]
    [ProducesOk<IReadOnlyList<ProjectApplicationDto>>, ProducesNotFound]
    public async Task<IActionResult> GetApplications([FromRoute] int id, CancellationToken ct)
    {
        var result = await memberService.GetApplicationsAsync(CurrentStudentId, id, ct);
        return OkResponse(result);
    }

    [HttpPost("{id:int}/applications")]
    [ProducesCreated<ProjectApplicationDto>]
    [ProducesNotFound, ProducesConflict]
    public async Task<IActionResult> Apply(
        [FromRoute] int id,
        [FromBody] CreateProjectApplicationRequest request,
        CancellationToken ct)
    {
        var result = await memberService.ApplyAsync(CurrentStudentId, id, request, ct);
        return CreatedResponse(nameof(GetApplications), new { id }, result, "Application submitted successfully.");
    }

    [HttpPatch("{id:int}/applications/{appId:int}")]
    [ProducesOk<ProjectApplicationDto>]
    [ProducesNotFound, ProducesConflict]
    public async Task<IActionResult> ReviewApplication(
        [FromRoute] int id,
        [FromRoute] int appId,
        [FromBody] ReviewProjectApplicationRequest request,
        CancellationToken ct)
    {
        var result = await memberService.ReviewApplicationAsync(CurrentStudentId, id, appId, request, ct);
        return OkResponse(result, "Application reviewed successfully.");
    }

    [HttpDelete("{id:int}/applications/{appId:int}")]
    [ProducesNoContent, ProducesNotFound]
    public async Task<IActionResult> WithdrawApplication([FromRoute] int id, [FromRoute] int appId, CancellationToken ct)
    {
        await memberService.WithdrawApplicationAsync(CurrentStudentId, id, appId, ct);
        return NoContentResponse();
    }

    [HttpGet("{id:int}/tasks")]
    [ProducesOk<IReadOnlyList<ProjectTaskDto>>, ProducesNotFound]
    public async Task<IActionResult> GetTasks([FromRoute] int id, CancellationToken ct)
    {
        var result = await taskService.GetProjectTasksAsync(CurrentStudentId, id, ct);
        return OkResponse(result);
    }

    [HttpPost("{id:int}/tasks")]
    [ProducesCreated<ProjectTaskDto>, ProducesNotFound]
    public async Task<IActionResult> CreateTask(
        [FromRoute] int id,
        [FromBody] CreateProjectTaskRequest request,
        CancellationToken ct)
    {
        var result = await taskService.CreateTaskAsync(CurrentStudentId, id, request, ct);
        return CreatedResponse(nameof(TasksController.GetTask), "Tasks", new { id = result.TaskId }, result, "Task created successfully.");
    }
}
