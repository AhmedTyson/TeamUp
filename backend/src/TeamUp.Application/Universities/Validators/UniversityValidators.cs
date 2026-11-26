// Date: 2026-11-21
// Purpose: FluentValidation rules for universities using SSRF/XSS-hardened URL validation (STRIDE THR-T-03).

using FluentValidation;
using TeamUp.Application.Common.Validation;

namespace TeamUp.Application.Universities.Validators;

public sealed class CreateUniversityRequestValidator : AbstractValidator<CreateUniversityRequest>
{
    public CreateUniversityRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("University name is required.")
            .MaximumLength(150).WithMessage("University name must not exceed 150 characters.");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(100).WithMessage("City must not exceed 100 characters.");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required.")
            .MaximumLength(100).WithMessage("Country must not exceed 100 characters.");

        RuleFor(x => x.Website)
            .MaximumLength(200)
            .Must(SafeUrlValidator.IsSafeExternalHttpUrl).WithMessage("Website must be a valid public HTTP or HTTPS URL.")
            .When(x => !string.IsNullOrWhiteSpace(x.Website));

        RuleFor(x => x.LogoUrl)
            .MaximumLength(500)
            .Must(SafeUrlValidator.IsSafeAssetOrExternalUrl)
            .WithMessage("Logo URL must be a valid relative asset path (/logos/...) or public HTTP/HTTPS URL.")
            .When(x => !string.IsNullOrWhiteSpace(x.LogoUrl));
    }
}

public sealed class UpdateUniversityRequestValidator : AbstractValidator<UpdateUniversityRequest>
{
    public UpdateUniversityRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("University name is required.")
            .MaximumLength(150).WithMessage("University name must not exceed 150 characters.");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(100).WithMessage("City must not exceed 100 characters.");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required.")
            .MaximumLength(100).WithMessage("Country must not exceed 100 characters.");

        RuleFor(x => x.Website)
            .MaximumLength(200)
            .Must(SafeUrlValidator.IsSafeExternalHttpUrl).WithMessage("Website must be a valid public HTTP or HTTPS URL.")
            .When(x => !string.IsNullOrWhiteSpace(x.Website));

        RuleFor(x => x.LogoUrl)
            .MaximumLength(500)
            .Must(SafeUrlValidator.IsSafeAssetOrExternalUrl)
            .WithMessage("Logo URL must be a valid relative asset path (/logos/...) or public HTTP/HTTPS URL.")
            .When(x => !string.IsNullOrWhiteSpace(x.LogoUrl));
    }
}
