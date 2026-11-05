using FluentValidation;

namespace TeamUp.Application.Skills.Validators;

public class CreateSkillRequestValidator : AbstractValidator<CreateSkillRequest>
{
    public CreateSkillRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Skill name is required.")
            .MaximumLength(60).WithMessage("Skill name cannot exceed 60 characters.");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Skill category is required.")
            .MaximumLength(50).WithMessage("Skill category cannot exceed 50 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(250).WithMessage("Description cannot exceed 250 characters.")
            .When(x => x.Description is not null);
    }
}

public class UpdateSkillRequestValidator : AbstractValidator<UpdateSkillRequest>
{
    public UpdateSkillRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Skill name is required.")
            .MaximumLength(60).WithMessage("Skill name cannot exceed 60 characters.");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Skill category is required.")
            .MaximumLength(50).WithMessage("Skill category cannot exceed 50 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(250).WithMessage("Description cannot exceed 250 characters.")
            .When(x => x.Description is not null);
    }
}

public class ClaimSkillRequestValidator : AbstractValidator<ClaimSkillRequest>
{
    public ClaimSkillRequestValidator()
    {
        RuleFor(x => x.ProficiencyLevel)
            .IsInEnum().WithMessage("Invalid proficiency level.");

        RuleFor(x => x.YearsOfExperience)
            .InclusiveBetween(0m, 50m).WithMessage("Years of experience must be between 0 and 50.");
    }
}
