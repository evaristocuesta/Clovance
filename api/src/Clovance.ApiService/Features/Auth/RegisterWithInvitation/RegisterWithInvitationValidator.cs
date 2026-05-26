using FluentValidation;
using Clovance.ApiService.Infrastructure.Validation;
using Clovance.ApiService.Shared;

namespace Clovance.ApiService.Features.Auth.RegisterWithInvitation;

public sealed class RegisterWithInvitationValidator : AbstractValidator<RegisterWithInvitationCommand>
{
    public RegisterWithInvitationValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Auth.EmailRequired)
            .EmailAddress()
            .WithErrorCode(ErrorCodes.Auth.EmailInvalid);

        RuleFor(x => x.Token)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Auth.InvitationTokenRequired);

        RuleFor(x => x.Password)
            .ApplyPasswordPolicy();
    }
}
