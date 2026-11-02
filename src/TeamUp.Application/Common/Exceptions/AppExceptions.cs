using TeamUp.Application.Common.Models;

namespace TeamUp.Application.Common.Exceptions;

public class NotFoundException(string message) : Exception(message);

public class ConflictException(string message) : Exception(message);

public class UnauthorizedException(string message) : Exception(message);

public class ForbiddenException(string message = "You do not have permission to access this resource.") : Exception(message);

public class BadRequestException(string message) : Exception(message);

public class ValidationException : Exception
{
    public IReadOnlyList<ErrorDetail> Errors { get; }

    public ValidationException(string message) : base(message)
    {
        Errors = [];
    }

    public ValidationException(IEnumerable<ErrorDetail> errors) : base("One or more validation failures occurred.")
    {
        Errors = errors.ToList();
    }

    public ValidationException(string field, string message) : base("One or more validation failures occurred.")
    {
        Errors = [new ErrorDetail(field, message)];
    }
}
