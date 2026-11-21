using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Api.Controllers.Common;
using TeamUp.Application.Projects;

namespace TeamUp.Api.Controllers;

[Route("api/v1/tasks")]
[Authorize]
public class TasksController(IProjectTaskService taskService) : BaseApiController
{
    [HttpGet("{id:int}")]
    [ProducesOk<ProjectTaskDto>, ProducesNotFound]
    public async Task<IActionResult> GetTask([FromRoute] int id, CancellationToken ct)
    {
        var result = await taskService.GetTaskAsync(CurrentStudentId, id, ct);
        return OkResponse(result);
    }

    [HttpPatch("{id:int}")]
    [ProducesOk<ProjectTaskDto>, ProducesNotFound]
    public async Task<IActionResult> UpdateTask(
        [FromRoute] int id,
        [FromBody] UpdateProjectTaskRequest request,
        CancellationToken ct)
    {
        var result = await taskService.UpdateTaskAsync(CurrentStudentId, id, request, ct);
        return OkResponse(result, "Task updated successfully.");
    }

    [HttpPut("{id:int}/assignee")]
    [ProducesNoContent, ProducesNotFound]
    public async Task<IActionResult> AssignTask(
        [FromRoute] int id,
        [FromBody] AssignTaskRequest request,
        CancellationToken ct)
    {
        await taskService.AssignTaskAsync(CurrentStudentId, id, request, ct);
        return NoContentResponse();
    }

    [HttpDelete("{id:int}/assignee")]
    [ProducesNoContent, ProducesNotFound]
    public async Task<IActionResult> UnassignTask([FromRoute] int id, CancellationToken ct)
    {
        await taskService.UnassignTaskAsync(CurrentStudentId, id, ct);
        return NoContentResponse();
    }

    [HttpDelete("{id:int}")]
    [ProducesNoContent, ProducesNotFound]
    public async Task<IActionResult> DeleteTask([FromRoute] int id, CancellationToken ct)
    {
        await taskService.DeleteTaskAsync(CurrentStudentId, id, ct);
        return NoContentResponse();
    }
}
