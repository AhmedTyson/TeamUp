using FluentValidation;
using TeamUp.Application.Common.Validation;

namespace TeamUp.Application.Events.Validators;

public sealed class CreateEventRequestValidator : AbstractValidator<CreateEventRequest>
{
    public CreateEventRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(5000);
        RuleFor(x => x.Location).NotEmpty().MaximumLength(200);
        RuleFor(x => x.OrganizationId).GreaterThan(0);
        RuleFor(x => x.Capacity).GreaterThanOrEqualTo(1);
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.DeliveryMode).IsInEnum();
        RuleFor(x => x.MeetingUrl)
            .MaximumLength(500)
            .Must(SafeUrlValidator.IsSafeExternalHttpUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.MeetingUrl))
            .WithMessage("MeetingUrl must be a valid public HTTP or HTTPS URL.");
    }
}

public sealed class UpdateEventRequestValidator : AbstractValidator<UpdateEventRequest>
{
    public UpdateEventRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(5000);
        RuleFor(x => x.Location).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Capacity).GreaterThanOrEqualTo(1);
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.DeliveryMode).IsInEnum();
        RuleFor(x => x.MeetingUrl)
            .MaximumLength(500)
            .Must(SafeUrlValidator.IsSafeExternalHttpUrl)
            .When(x => !string.IsNullOrWhiteSpace(x.MeetingUrl))
            .WithMessage("MeetingUrl must be a valid public HTTP or HTTPS URL.");
    }
}

public sealed class CreateEventSessionRequestValidator : AbstractValidator<CreateEventSessionRequest>
{
    public CreateEventSessionRequestValidator()
    {
        RuleFor(x => x.SessionNo).GreaterThan(0);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(120);
        RuleFor(x => x.EndTime).GreaterThanOrEqualTo(x => x.StartTime);
        RuleFor(x => x.Room).MaximumLength(60);
    }
}

public sealed class UpdateEventSessionRequestValidator : AbstractValidator<UpdateEventSessionRequest>
{
    public UpdateEventSessionRequestValidator()
    {
        RuleFor(x => x.SessionNo).GreaterThan(0);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(120);
        RuleFor(x => x.EndTime).GreaterThanOrEqualTo(x => x.StartTime);
        RuleFor(x => x.Room).MaximumLength(60);
    }
}


public sealed class CreateAttendanceRequestValidator : AbstractValidator<CreateAttendanceRequest>
{
    public CreateAttendanceRequestValidator()
    {
        RuleFor(x => x.StudentId).GreaterThan(0);
        RuleFor(x => x.VerificationMethod).IsInEnum();
        RuleFor(x => x.VerificationCode)
            .MaximumLength(128)
            .When(x => !string.IsNullOrWhiteSpace(x.VerificationCode));
    }
}

public sealed class SelfCheckInRequestValidator : AbstractValidator<SelfCheckInRequest>
{
    public SelfCheckInRequestValidator()
    {
        RuleFor(x => x.VerificationCode)
            .NotEmpty().WithMessage("Verification code is required.")
            .MaximumLength(128);
    }
}
