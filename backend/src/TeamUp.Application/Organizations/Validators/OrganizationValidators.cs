using FluentValidation;

namespace TeamUp.Application.Organizations.Validators;

public sealed class CreateOrganizationRequestValidator : AbstractValidator<CreateOrganizationRequest>
{
    public CreateOrganizationRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LogoUrl).Must(BeValidUrl).When(x => !string.IsNullOrWhiteSpace(x.LogoUrl))
            .WithMessage("LogoUrl must be a valid HTTP or HTTPS URL.");
    }

    private static bool BeValidUrl(string? value) => Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}

public sealed class UpdateOrganizationRequestValidator : AbstractValidator<UpdateOrganizationRequest>
{
    public UpdateOrganizationRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LogoUrl).Must(value => Uri.TryCreate(value, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            .When(x => !string.IsNullOrWhiteSpace(x.LogoUrl))
            .WithMessage("LogoUrl must be a valid HTTP or HTTPS URL.");
    }
}

public sealed class UpdateOrganizationMemberRequestValidator : AbstractValidator<UpdateOrganizationMemberRequest>
{
    public UpdateOrganizationMemberRequestValidator()
    {
        RuleFor(x => x.Role).NotEmpty().MaximumLength(60);
    }
}

public sealed class CreateMembershipApplicationRequestValidator : AbstractValidator<CreateMembershipApplicationRequest>
{
    public CreateMembershipApplicationRequestValidator()
    {
        RuleFor(x => x.Message).MaximumLength(600);
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
