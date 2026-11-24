using FluentValidation;

namespace TeamUp.Application.Admin.Validators;

public class UpdateUserRoleRequestValidator : AbstractValidator<UpdateUserRoleRequest>
{
    public UpdateUserRoleRequestValidator()
    {
        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("A valid UserRole (Student, OrgAdmin, PlatformAdmin) is required.");
    }
}

public class SendAdminReportRequestValidator : AbstractValidator<SendAdminReportRequest>
{
    public SendAdminReportRequestValidator()
    {
        RuleFor(x => x.RecipientEmail)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.RecipientEmail))
            .MaximumLength(150);

        RuleFor(x => x.ReportPeriod)
            .MaximumLength(120);
    }
}

public class AdminLeaderboardDigestCommandValidator : AbstractValidator<AdminLeaderboardDigestCommand>
{
    public AdminLeaderboardDigestCommandValidator()
    {
        RuleFor(x => x.Mode)
            .NotEmpty()
            .Must(m => m.Equals("TestToMe", StringComparison.OrdinalIgnoreCase)
                || m.Equals("Broadcast", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Mode must be either 'TestToMe' or 'Broadcast'.");

        RuleFor(x => x.PeriodLabel)
            .MaximumLength(120);

        RuleForEach(x => x.NewsItems).ChildRules(item =>
        {
            item.RuleFor(i => i.Category).NotEmpty().MaximumLength(60);
            item.RuleFor(i => i.Headline).NotEmpty().MaximumLength(180);
            item.RuleFor(i => i.Summary).NotEmpty().MaximumLength(500);
        });
    }
}

public class AdminProjectMatchDigestCommandValidator : AbstractValidator<AdminProjectMatchDigestCommand>
{
    public AdminProjectMatchDigestCommandValidator()
    {
        RuleFor(x => x.Mode)
            .NotEmpty()
            .Must(m => m.Equals("TestToMe", StringComparison.OrdinalIgnoreCase)
                || m.Equals("Broadcast", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Mode must be either 'TestToMe' or 'Broadcast'.");

        RuleFor(x => x.MaxMatchesPerStudent).InclusiveBetween(1, 10);
        RuleFor(x => x.MinMatchPercentage).InclusiveBetween(0, 100);
    }
}
