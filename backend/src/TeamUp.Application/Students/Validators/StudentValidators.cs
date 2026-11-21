using FluentValidation;

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
    }
}

public class UpsertProfileRequestValidator : AbstractValidator<UpsertProfileRequest>
{
    public UpsertProfileRequestValidator()
    {
        RuleFor(x => x.Bio)
            .MaximumLength(1000).WithMessage("Bio must not exceed 1000 characters.");

        RuleFor(x => x.PhotoUrl)
            .Must(BeValidUrl).WithMessage("Photo URL must be a valid HTTP or HTTPS URL.")
            .When(x => !string.IsNullOrWhiteSpace(x.PhotoUrl));

        RuleFor(x => x.GitHubUrl)
            .Must(BeValidUrl).WithMessage("GitHub URL must be a valid HTTP or HTTPS URL.")
            .When(x => !string.IsNullOrWhiteSpace(x.GitHubUrl));

        RuleFor(x => x.LinkedInUrl)
            .Must(BeValidUrl).WithMessage("LinkedIn URL must be a valid HTTP or HTTPS URL.")
            .When(x => !string.IsNullOrWhiteSpace(x.LinkedInUrl));
    }

    private static bool BeValidUrl(string? url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
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
