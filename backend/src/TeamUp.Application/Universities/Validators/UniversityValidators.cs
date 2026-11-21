using FluentValidation;

namespace TeamUp.Application.Universities.Validators;

public class CreateUniversityRequestValidator : AbstractValidator<CreateUniversityRequest>
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
            .Must(BeValidUrl).WithMessage("Website must be a valid HTTP or HTTPS URL.")
            .When(x => !string.IsNullOrWhiteSpace(x.Website));
    }

    private static bool BeValidUrl(string? url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}

public class UpdateUniversityRequestValidator : AbstractValidator<UpdateUniversityRequest>
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
            .Must(BeValidUrl).WithMessage("Website must be a valid HTTP or HTTPS URL.")
            .When(x => !string.IsNullOrWhiteSpace(x.Website));
    }

    private static bool BeValidUrl(string? url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
