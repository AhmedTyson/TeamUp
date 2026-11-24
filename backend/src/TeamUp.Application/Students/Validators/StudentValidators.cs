// Date: 2026-11-21
// Purpose: FluentValidation rules for student account, portfolio URLs (SSRF/XSS hardened), and interests.

using FluentValidation;
using TeamUp.Application.Common.Validation;

namespace TeamUp.Application.Students.Validators;

public class UpdateStudentRequestValidator : AbstractValidator<UpdateStudentRequest>
{
    public UpdateStudentRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .MaximumLength(50).WithMessage("First name must not exceed 50 characters.")
            .When(x => !string.IsNullOrEmpty(x.FirstName));

        RuleFor(x => x.LastName)
            .MaximumLength(50).WithMessage("Last name must not exceed 50 characters.")
            .When(x => !string.IsNullOrEmpty(x.LastName));

        RuleFor(x => x.AcademicYear)
            .InclusiveBetween(1, 7).WithMessage("Academic year must be between 1 and 7.")
            .When(x => x.AcademicYear.HasValue);

        RuleFor(x => x.AffiliationType)
            .IsInEnum().WithMessage("Affiliation type is invalid.")
            .When(x => x.AffiliationType.HasValue);

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("City must not exceed 100 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.City));
    }
}

public class UpsertProfileRequestValidator : AbstractValidator<UpsertProfileRequest>
{
    public UpsertProfileRequestValidator()
    {
        RuleFor(x => x.Bio)
            .MaximumLength(1000).WithMessage("Bio must not exceed 1000 characters.");

        RuleFor(x => x.PhotoUrl)
            .MaximumLength(300)
            .Must(SafeUrlValidator.IsSafeAssetOrExternalUrl)
            .WithMessage("Photo URL must be a valid public relative asset path (/images/...) or HTTP/HTTPS URL.")
            .When(x => !string.IsNullOrWhiteSpace(x.PhotoUrl));

        RuleFor(x => x.GitHubUrl)
            .MaximumLength(200)
            .Must(SafeUrlValidator.IsSafeExternalHttpUrl).WithMessage("GitHub URL must be a valid public HTTP or HTTPS URL.")
            .When(x => !string.IsNullOrWhiteSpace(x.GitHubUrl));

        RuleFor(x => x.LinkedInUrl)
            .MaximumLength(200)
            .Must(SafeUrlValidator.IsSafeExternalHttpUrl).WithMessage("LinkedIn URL must be a valid public HTTP or HTTPS URL.")
            .When(x => !string.IsNullOrWhiteSpace(x.LinkedInUrl));
    }
}

public class AddInterestRequestValidator : AbstractValidator<AddInterestRequest>
{
    public AddInterestRequestValidator()
    {
        RuleFor(x => x.Interest)
            .NotEmpty().WithMessage("Interest tag cannot be empty.")
            .MaximumLength(50).WithMessage("Interest tag must not exceed 50 characters.");
    }
}
