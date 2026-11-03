using FluentValidation;

namespace TeamUp.Application.Universities.Validators;

public class CreateFacultyRequestValidator : AbstractValidator<CreateFacultyRequest>
{
    public CreateFacultyRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Faculty name is required.")
            .MaximumLength(150).WithMessage("Faculty name must not exceed 150 characters.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Faculty academic code is required.")
            .MaximumLength(20).WithMessage("Faculty code must not exceed 20 characters.")
            .Matches("^[A-Za-z0-9_-]+$").WithMessage("Faculty code may only contain letters, numbers, underscores, and dashes.");
    }
}

public class UpdateFacultyRequestValidator : AbstractValidator<UpdateFacultyRequest>
{
    public UpdateFacultyRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Faculty name is required.")
            .MaximumLength(150).WithMessage("Faculty name must not exceed 150 characters.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Faculty academic code is required.")
            .MaximumLength(20).WithMessage("Faculty code must not exceed 20 characters.")
            .Matches("^[A-Za-z0-9_-]+$").WithMessage("Faculty code may only contain letters, numbers, underscores, and dashes.");
    }
}
