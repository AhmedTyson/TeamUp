using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Application.Common;
using TeamUp.Application.Common.Security;
using TeamUp.Application.Projects;

namespace TeamUp.Api.Controllers;

[ApiController]
[Route("api/v1/projects")]
[Authorize]
public class ProjectsController(IProjectService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProjectListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProjects([FromQuery] string? keyword, [FromQuery] string? status, [FromQuery] int? skillId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        => Ok(await service.GetProjectsAsync(keyword, status, skillId, new PageRequest(page, pageSize), ct));

    [HttpPost]
    [ProducesResponseType(typeof(ProjectDetailDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request, CancellationToken ct)
    {
        var result = await service.CreateProjectAsync(User.GetStudentId(), request, ct);
        return CreatedAtAction(nameof(GetProject), new { id = result.ProjectId }, result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProjectDetailDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProject([FromRoute] int id, CancellationToken ct) => Ok(await service.GetProjectAsync(id, ct));

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> UpdateProject([FromRoute] int id, [FromBody] UpdateProjectRequest request, CancellationToken ct)
        => Ok(await service.UpdateProjectAsync(User.GetStudentId(), id, request, ct));

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteProject([FromRoute] int id, CancellationToken ct)
    { await service.DeleteProjectAsync(User.GetStudentId(), id, ct); return NoContent(); }

    [HttpGet("{id:int}/required-skills")]
    public async Task<IActionResult> GetRequiredSkills([FromRoute] int id, CancellationToken ct) => Ok(await service.GetRequiredSkillsAsync(id, ct));

    [HttpPut("{id:int}/required-skills/{skillId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpsertRequiredSkill([FromRoute] int id, [FromRoute] int skillId, [FromBody] UpsertRequiredSkillRequest request, CancellationToken ct)
    { await service.UpsertRequiredSkillAsync(User.GetStudentId(), id, skillId, request, ct); return NoContent(); }

    [HttpDelete("{id:int}/required-skills/{skillId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveRequiredSkill([FromRoute] int id, [FromRoute] int skillId, CancellationToken ct)
    { await service.RemoveRequiredSkillAsync(User.GetStudentId(), id, skillId, ct); return NoContent(); }

    [HttpGet("{id:int}/members")]
    public async Task<IActionResult> GetMembers([FromRoute] int id, CancellationToken ct) => Ok(await service.GetMembersAsync(id, ct));

    [HttpPatch("{id:int}/members/{studentId:int}")]
    public async Task<IActionResult> UpdateMemberRole([FromRoute] int id, [FromRoute] int studentId, [FromBody] UpdateProjectMemberRequest request, CancellationToken ct)
    { await service.UpdateMemberRoleAsync(User.GetStudentId(), id, studentId, request, ct); return NoContent(); }

    [HttpDelete("{id:int}/members/{studentId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveMember([FromRoute] int id, [FromRoute] int studentId, CancellationToken ct)
    { await service.RemoveMemberAsync(User.GetStudentId(), id, studentId, ct); return NoContent(); }

    [HttpGet("{id:int}/applications")]
    public async Task<IActionResult> GetApplications([FromRoute] int id, CancellationToken ct)
        => Ok(await service.GetApplicationsAsync(User.GetStudentId(), id, ct));

    [HttpPost("{id:int}/applications")]
    [ProducesResponseType(typeof(ProjectApplicationDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Apply([FromRoute] int id, [FromBody] CreateProjectApplicationRequest request, CancellationToken ct)
    {
        var result = await service.ApplyAsync(User.GetStudentId(), id, request, ct);
        return CreatedAtAction(nameof(GetApplications), new { id }, result);
    }

    [HttpPatch("{id:int}/applications/{appId:int}")]
    public async Task<IActionResult> ReviewApplication([FromRoute] int id, [FromRoute] int appId, [FromBody] ReviewProjectApplicationRequest request, CancellationToken ct)
        => Ok(await service.ReviewApplicationAsync(User.GetStudentId(), id, appId, request, ct));

    [HttpDelete("{id:int}/applications/{appId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> WithdrawApplication([FromRoute] int id, [FromRoute] int appId, CancellationToken ct)
    { await service.WithdrawApplicationAsync(User.GetStudentId(), id, appId, ct); return NoContent(); }

    [HttpGet("{id:int}/tasks")]
    public async Task<IActionResult> GetTasks([FromRoute] int id, CancellationToken ct)
        => Ok(await service.GetProjectTasksAsync(User.GetStudentId(), id, ct));

    [HttpPost("{id:int}/tasks")]
    [ProducesResponseType(typeof(ProjectTaskDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateTask([FromRoute] int id, [FromBody] CreateProjectTaskRequest request, CancellationToken ct)
    {
        var result = await service.CreateTaskAsync(User.GetStudentId(), id, request, ct);
        return CreatedAtAction(nameof(TasksController.GetTask), "Tasks", new { id = result.TaskId }, result);
    }
}
