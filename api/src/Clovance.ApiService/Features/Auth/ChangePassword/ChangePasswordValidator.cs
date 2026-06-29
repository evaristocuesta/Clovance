using Clovance.ApiService.Infrastructure.Validation;
using Clovance.ApiService.Shared;
using FluentValidation;

namespace Clovance.ApiService.Features.Auth.ChangePassword;

public class ChangePasswordValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Auth.CurrentPasswordRequired);
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Auth.NewPasswordRequired)
            .ApplyPasswordPolicy();
    }
}
