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
