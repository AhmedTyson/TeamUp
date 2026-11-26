// Date: 2026-11-21
// Purpose: FluentValidation rules for badges, score transactions, and notifications with SSRF/XSS-safe IconUrl validation (STRIDE THR-T-03).

using FluentValidation;
using TeamUp.Application.Common.Validation;

namespace TeamUp.Application.Gamification.Validators;

public sealed class CreateBadgeRequestValidator : AbstractValidator<CreateBadgeRequest>
{
    public CreateBadgeRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Badge name is required.")
            .MaximumLength(80).WithMessage("Badge name cannot exceed 80 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Badge description is required.")
            .MaximumLength(250).WithMessage("Badge description cannot exceed 250 characters.");

        RuleFor(x => x.IconUrl)
            .NotEmpty().WithMessage("Badge icon URL is required.")
            .MaximumLength(300).WithMessage("Icon URL cannot exceed 300 characters.")
            .Must(SafeUrlValidator.IsSafeAssetOrExternalUrl)
            .WithMessage("Badge icon URL must be a safe relative asset path or a valid public HTTP/HTTPS URL.");

        RuleFor(x => x.CriteriaType)
            .IsInEnum().WithMessage("Invalid badge criteria type.");

        RuleFor(x => x.CriteriaValue)
            .GreaterThanOrEqualTo(0).WithMessage("Criteria value must be non-negative.");
    }
}

public sealed class UpdateBadgeRequestValidator : AbstractValidator<UpdateBadgeRequest>
{
    public UpdateBadgeRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Badge name cannot be empty.")
            .MaximumLength(80).WithMessage("Badge name cannot exceed 80 characters.")
            .When(x => x.Name is not null);

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Badge description cannot be empty.")
            .MaximumLength(250).WithMessage("Badge description cannot exceed 250 characters.")
            .When(x => x.Description is not null);

        RuleFor(x => x.IconUrl)
            .NotEmpty().WithMessage("Badge icon URL cannot be empty.")
            .MaximumLength(300).WithMessage("Icon URL cannot exceed 300 characters.")
            .Must(SafeUrlValidator.IsSafeAssetOrExternalUrl)
            .WithMessage("Badge icon URL must be a safe relative asset path or a valid public HTTP/HTTPS URL.")
            .When(x => x.IconUrl is not null);

        RuleFor(x => x.CriteriaType)
            .IsInEnum().WithMessage("Invalid badge criteria type.")
            .When(x => x.CriteriaType.HasValue);

        RuleFor(x => x.CriteriaValue)
            .GreaterThanOrEqualTo(0).WithMessage("Criteria value must be non-negative.")
            .When(x => x.CriteriaValue.HasValue);
    }
}

public sealed class CreateScoreTransactionRequestValidator : AbstractValidator<CreateScoreTransactionRequest>
{
    public CreateScoreTransactionRequestValidator()
    {
        RuleFor(x => x.StudentId)
            .GreaterThan(0).WithMessage("Valid student ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required.")
            .MaximumLength(200).WithMessage("Reason cannot exceed 200 characters.");

        RuleFor(x => x.SourceType)
            .IsInEnum().WithMessage("Invalid score source type.");
    }
}

public sealed class CreateNotificationRequestValidator : AbstractValidator<CreateNotificationRequest>
{
    public CreateNotificationRequestValidator()
    {
        RuleFor(x => x.StudentId)
            .GreaterThan(0).WithMessage("Valid student ID is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(120).WithMessage("Title cannot exceed 120 characters.");

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required.")
            .MaximumLength(500).WithMessage("Message cannot exceed 500 characters.");
    }
}
