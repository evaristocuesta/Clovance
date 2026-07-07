using Clovance.ApiService.Infrastructure.Validation;
using Clovance.ApiService.Shared;
using FluentValidation;

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

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Auth.FirstName.Required)
            .MaximumLength(100)
            .WithErrorCode(ErrorCodes.Auth.FirstName.MaxLength);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Auth.LastName.Required)
            .MaximumLength(100)
            .WithErrorCode(ErrorCodes.Auth.LastName.MaxLength);
    }
}
