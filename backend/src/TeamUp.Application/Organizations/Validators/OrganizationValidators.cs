// Date: 2026-11-21
// Purpose: FluentValidation rules for organizations and membership applications using SSRF-safe URL validation (STRIDE THR-T-03).

using FluentValidation;
using TeamUp.Application.Common.Validation;

namespace TeamUp.Application.Organizations.Validators;

public sealed class CreateOrganizationRequestValidator : AbstractValidator<CreateOrganizationRequest>
{
    public CreateOrganizationRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LogoUrl)
            .MaximumLength(300)
            .Must(SafeUrlValidator.IsSafeAssetOrExternalUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.LogoUrl))
            .WithMessage("LogoUrl must be a valid asset path or public HTTP/HTTPS URL.");
        RuleFor(x => x.Scope).IsInEnum();
        RuleFor(x => x.City).MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.City));
        RuleFor(x => x.CurrentSeasonName).MaximumLength(80).When(x => !string.IsNullOrWhiteSpace(x.CurrentSeasonName));
        RuleFor(x => x.CoverImageUrl)
            .MaximumLength(500)
            .Must(SafeUrlValidator.IsSafeAssetOrExternalUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.CoverImageUrl))
            .WithMessage("CoverImageUrl must be a valid asset path or public HTTP/HTTPS URL.");
        RuleFor(x => x.WebsiteUrl)
            .MaximumLength(300)
            .Must(SafeUrlValidator.IsSafeExternalHttpUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.WebsiteUrl))
            .WithMessage("WebsiteUrl must be a valid public HTTP or HTTPS URL.");
    }
}

public sealed class UpdateOrganizationRequestValidator : AbstractValidator<UpdateOrganizationRequest>
{
    public UpdateOrganizationRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LogoUrl)
            .MaximumLength(300)
            .Must(SafeUrlValidator.IsSafeAssetOrExternalUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.LogoUrl))
            .WithMessage("LogoUrl must be a valid asset path or public HTTP/HTTPS URL.");
        RuleFor(x => x.Scope).IsInEnum();
        RuleFor(x => x.City).MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.City));
        RuleFor(x => x.CurrentSeasonName).MaximumLength(80).When(x => !string.IsNullOrWhiteSpace(x.CurrentSeasonName));
        RuleFor(x => x.CoverImageUrl)
            .MaximumLength(500)
            .Must(SafeUrlValidator.IsSafeAssetOrExternalUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.CoverImageUrl))
            .WithMessage("CoverImageUrl must be a valid asset path or public HTTP/HTTPS URL.");
        RuleFor(x => x.WebsiteUrl)
            .MaximumLength(300)
            .Must(SafeUrlValidator.IsSafeExternalHttpUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.WebsiteUrl))
            .WithMessage("WebsiteUrl must be a valid public HTTP or HTTPS URL.");
    }
}

public sealed class UpdateOrganizationMemberRequestValidator : AbstractValidator<UpdateOrganizationMemberRequest>
{
    public UpdateOrganizationMemberRequestValidator()
    {
        RuleFor(x => x.Role).NotEmpty().MaximumLength(60);
        RuleFor(x => x.HierarchyTier).IsInEnum().When(x => x.HierarchyTier.HasValue);
    }
}

public sealed class CreateOrganizationUnitRequestValidator : AbstractValidator<CreateOrganizationUnitRequest>
{
    public CreateOrganizationUnitRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Description).MaximumLength(600);
        RuleFor(x => x.UnitType).IsInEnum();
        RuleFor(x => x.OpenSeats).GreaterThanOrEqualTo(0).When(x => x.OpenSeats.HasValue);
    }
}

public sealed class UpdateOrganizationUnitRequestValidator : AbstractValidator<UpdateOrganizationUnitRequest>
{
    public UpdateOrganizationUnitRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Description).MaximumLength(600);
        RuleFor(x => x.UnitType).IsInEnum();
        RuleFor(x => x.OpenSeats).GreaterThanOrEqualTo(0).When(x => x.OpenSeats.HasValue);
    }
}

public sealed class CreateMembershipApplicationRequestValidator : AbstractValidator<CreateMembershipApplicationRequest>
{
    public CreateMembershipApplicationRequestValidator()
    {
        RuleFor(x => x.Message).MaximumLength(600);
        RuleFor(x => x.SeasonName).MaximumLength(80);
    }
}

public sealed class ReviewMembershipApplicationRequestValidator : AbstractValidator<ReviewMembershipApplicationRequest>
{
    public ReviewMembershipApplicationRequestValidator()
    {
        RuleFor(x => x.Status).Must(status => status is TeamUp.Domain.Enums.ApplicationStatus.Accepted
            or TeamUp.Domain.Enums.ApplicationStatus.Rejected)
            .WithMessage("Status must be Accepted or Rejected.");
    }
}

public sealed class UpdateApplicationStageRequestValidator : AbstractValidator<UpdateApplicationStageRequest>
{
    public UpdateApplicationStageRequestValidator()
    {
        RuleFor(x => x.Stage).IsInEnum();
        RuleFor(x => x.EvaluationScore).InclusiveBetween(0m, 10m).When(x => x.EvaluationScore.HasValue);
        RuleFor(x => x.InterviewNotes).MaximumLength(1000);
    }
}

public sealed class CreateOrganizationProposalRequestValidator : AbstractValidator<CreateOrganizationProposalRequest>
{
    public CreateOrganizationProposalRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LogoUrl)
            .MaximumLength(300)
            .Must(SafeUrlValidator.IsSafeAssetOrExternalUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.LogoUrl))
            .WithMessage("LogoUrl must be a valid asset path or public HTTP/HTTPS URL.");
        RuleFor(x => x.Scope).IsInEnum();
        RuleFor(x => x.City).MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.City));
        RuleFor(x => x.CurrentSeasonName).MaximumLength(80).When(x => !string.IsNullOrWhiteSpace(x.CurrentSeasonName));
        RuleFor(x => x.CoverImageUrl)
            .MaximumLength(500)
            .Must(SafeUrlValidator.IsSafeAssetOrExternalUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.CoverImageUrl))
            .WithMessage("CoverImageUrl must be a valid asset path or public HTTP/HTTPS URL.");
        RuleFor(x => x.WebsiteUrl)
            .MaximumLength(300)
            .Must(SafeUrlValidator.IsSafeExternalHttpUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.WebsiteUrl))
            .WithMessage("WebsiteUrl must be a valid public HTTP or HTTPS URL.");
        RuleFor(x => x.Justification).MaximumLength(1000).When(x => !string.IsNullOrWhiteSpace(x.Justification));
    }
}

public sealed class ReviewOrganizationProposalRequestValidator : AbstractValidator<ReviewOrganizationProposalRequest>
{
    public ReviewOrganizationProposalRequestValidator()
    {
        RuleFor(x => x.ReviewNotes).MaximumLength(1000).When(x => !string.IsNullOrWhiteSpace(x.ReviewNotes));
    }
}
