using FluentValidation;

namespace TeamUp.Application.Gamification.Validators;

public class CreateBadgeRequestValidator : AbstractValidator<CreateBadgeRequest>
{
    public CreateBadgeRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Badge name is required.")
            .MaximumLength(120).WithMessage("Badge name cannot exceed 120 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Badge description is required.");

        RuleFor(x => x.IconUrl)
            .NotEmpty().WithMessage("Badge icon URL is required.")
            .MaximumLength(500).WithMessage("Icon URL cannot exceed 500 characters.");

        RuleFor(x => x.CriteriaValue)
            .GreaterThanOrEqualTo(0).WithMessage("Criteria value must be non-negative.");
    }
}

public class CreateScoreTransactionRequestValidator : AbstractValidator<CreateScoreTransactionRequest>
{
    public CreateScoreTransactionRequestValidator()
    {
        RuleFor(x => x.StudentId)
            .GreaterThan(0).WithMessage("Valid student ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required.")
            .MaximumLength(255).WithMessage("Reason cannot exceed 255 characters.");
    }
}

public class CreateNotificationRequestValidator : AbstractValidator<CreateNotificationRequest>
{
    public CreateNotificationRequestValidator()
    {
        RuleFor(x => x.StudentId)
            .GreaterThan(0).WithMessage("Valid student ID is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required.");
    }
}
