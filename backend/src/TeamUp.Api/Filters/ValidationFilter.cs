using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeamUp.Application.Common.Models;

namespace TeamUp.Api.Filters;

/// <summary>
/// Automatically executes registered FluentValidators on action arguments and returns structured 400 Bad Request if invalid.
/// </summary>
public sealed class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var services = context.HttpContext.RequestServices;

        foreach (var (_, arg) in context.ActionArguments)
        {
            if (arg is null) continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(arg.GetType());
            if (services.GetService(validatorType) is not IValidator validator) continue;

            var validationContext = new ValidationContext<object>(arg);
            var result = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);

            if (!result.IsValid)
            {
                var errors = result.Errors
                    .Select(e => new ErrorDetail(ToCamelCase(e.PropertyName), e.ErrorMessage))
                    .ToList();

                var errorResponse = BaseResponse.Fail(
                    "Validation failed: Please correct the highlighted fields and try again.",
                    StatusCodes.Status400BadRequest,
                    errors);

                context.Result = new BadRequestObjectResult(errorResponse);
                return;
            }
        }

        await next();
    }

    private static string ToCamelCase(string str)
    {
        if (string.IsNullOrEmpty(str) || char.IsLower(str[0]))
            return str;
        return char.ToLowerInvariant(str[0]) + str[1..];
    }
}
