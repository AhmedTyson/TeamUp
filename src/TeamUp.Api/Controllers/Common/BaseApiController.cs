using Microsoft.AspNetCore.Mvc;
using TeamUp.Application.Common.Models;
using TeamUp.Application.Common.Security;

namespace TeamUp.Api.Controllers.Common;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected int CurrentStudentId => User.GetStudentId();
    protected string? CurrentUserRole => User.GetUserRole();

    protected IActionResult OkResponse<T>(T data, string message = "Success") =>
        Ok(BaseResponse<T>.Ok(data, message, StatusCodes.Status200OK));

    protected IActionResult OkMessage(string message = "Success") =>
        Ok(BaseResponse.Ok(message, StatusCodes.Status200OK));

    protected IActionResult CreatedResponse<T>(T data, string message = "Resource created successfully.") =>
        StatusCode(StatusCodes.Status201Created, BaseResponse<T>.Ok(data, message, StatusCodes.Status201Created));

    protected IActionResult CreatedResponse<T>(
        string actionName, object? routeValues, T data, string message = "Resource created successfully.") =>
        CreatedAtAction(actionName, routeValues, BaseResponse<T>.Ok(data, message, StatusCodes.Status201Created));

    protected IActionResult NoContentResponse() => NoContent();
}
