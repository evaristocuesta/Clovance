using Clovance.ApiService.Infrastructure.Validation;
using Clovance.ApiService.Shared;
using FluentValidation;

namespace Clovance.ApiService.Features.Auth.ResetPassword;

public class ResetPasswordValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Auth.EmailRequired)
            .EmailAddress()
            .WithErrorCode(ErrorCodes.Auth.EmailInvalid);

        RuleFor(x => x.Token)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Auth.PasswordResetTokenRequired);

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Auth.NewPasswordRequired)
            .ApplyPasswordPolicy();
    }
}
