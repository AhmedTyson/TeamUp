using FluentValidation;
using TeamUp.Domain.Enums;

namespace TeamUp.Application.Projects.Validators;

public class CreateProjectRequestValidator : AbstractValidator<CreateProjectRequest>
{
    public CreateProjectRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Project title is required.")
            .MaximumLength(150).WithMessage("Project title must not exceed 150 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Project description is required.");

        RuleFor(x => x.MaxTeamSize)
            .InclusiveBetween(1, 20).WithMessage("Max team size must be between 1 and 20.");
    }
}

public class UpdateProjectRequestValidator : AbstractValidator<UpdateProjectRequest>
{
    public UpdateProjectRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Project title cannot be empty.")
            .MaximumLength(150).WithMessage("Project title must not exceed 150 characters.")
            .When(x => x.Title is not null);

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Project description cannot be empty.")
            .When(x => x.Description is not null);

        RuleFor(x => x.MaxTeamSize)
            .InclusiveBetween(1, 20).WithMessage("Max team size must be between 1 and 20.")
            .When(x => x.MaxTeamSize.HasValue);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid project status.")
            .When(x => x.Status.HasValue);
    }
}

public class UpsertRequiredSkillRequestValidator : AbstractValidator<UpsertRequiredSkillRequest>
{
    public UpsertRequiredSkillRequestValidator()
    {
        RuleFor(x => x.RequiredLevel)
            .IsInEnum().WithMessage("Invalid required proficiency level.");
    }
}

public class UpdateProjectMemberRequestValidator : AbstractValidator<UpdateProjectMemberRequest>
{
    public UpdateProjectMemberRequestValidator()
    {
        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Member role is required.")
            .MaximumLength(60).WithMessage("Member role cannot exceed 60 characters.");
    }
}

public class CreateProjectApplicationRequestValidator : AbstractValidator<CreateProjectApplicationRequest>
{
    public CreateProjectApplicationRequestValidator()
    {
        RuleFor(x => x.Message)
            .MaximumLength(500).WithMessage("Application message cannot exceed 500 characters.");
    }
}

public class ReviewProjectApplicationRequestValidator : AbstractValidator<ReviewProjectApplicationRequest>
{
    public ReviewProjectApplicationRequestValidator()
    {
        RuleFor(x => x.Status)
            .Must(s => s is ApplicationStatus.Accepted or ApplicationStatus.Rejected)
            .WithMessage("Application status review must be either Accepted or Rejected.");
    }
}

public class CreateProjectTaskRequestValidator : AbstractValidator<CreateProjectTaskRequest>
{
    public CreateProjectTaskRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task title is required.")
            .MaximumLength(150).WithMessage("Task title cannot exceed 150 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Task description cannot exceed 1000 characters.");

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Invalid task priority.");
    }
}

public class UpdateProjectTaskRequestValidator : AbstractValidator<UpdateProjectTaskRequest>
{
    public UpdateProjectTaskRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task title cannot be empty.")
            .MaximumLength(150).WithMessage("Task title cannot exceed 150 characters.")
            .When(x => x.Title is not null);

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Task description cannot exceed 1000 characters.")
            .When(x => x.Description is not null);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid task status.")
            .When(x => x.Status.HasValue);

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Invalid task priority.")
            .When(x => x.Priority.HasValue);
    }
}

public class AssignTaskRequestValidator : AbstractValidator<AssignTaskRequest>
{
    public AssignTaskRequestValidator()
    {
        RuleFor(x => x.StudentId)
            .GreaterThan(0).WithMessage("A valid student ID is required for task assignment.");
    }
}
