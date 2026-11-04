using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamUp.Application.Common.Security;
using TeamUp.Application.Projects;

namespace TeamUp.Api.Controllers;

[ApiController]
[Route("api/v1/tasks")]
[Authorize]
public class TasksController(IProjectService service) : ControllerBase
{
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProjectTaskDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTask([FromRoute] int id, CancellationToken ct)
        => Ok(await service.GetTaskAsync(User.GetStudentId(), id, ct));

    [HttpPatch("{id:int}")]
    [ProducesResponseType(typeof(ProjectTaskDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateTask([FromRoute] int id, [FromBody] UpdateProjectTaskRequest request, CancellationToken ct)
        => Ok(await service.UpdateTaskAsync(User.GetStudentId(), id, request, ct));

    [HttpPut("{id:int}/assignee")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AssignTask([FromRoute] int id, [FromBody] AssignTaskRequest request, CancellationToken ct)
    { await service.AssignTaskAsync(User.GetStudentId(), id, request, ct); return NoContent(); }

    [HttpDelete("{id:int}/assignee")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UnassignTask([FromRoute] int id, CancellationToken ct)
    { await service.UnassignTaskAsync(User.GetStudentId(), id, ct); return NoContent(); }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteTask([FromRoute] int id, CancellationToken ct)
    { await service.DeleteTaskAsync(User.GetStudentId(), id, ct); return NoContent(); }
}
