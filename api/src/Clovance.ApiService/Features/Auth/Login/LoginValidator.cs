using Clovance.ApiService.Shared;
using FluentValidation;

namespace Clovance.ApiService.Features.Auth.Login;

public sealed class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Auth.EmailRequired)
            .EmailAddress()
            .WithErrorCode(ErrorCodes.Auth.EmailInvalid);

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Auth.PasswordRequired);
    }
}
