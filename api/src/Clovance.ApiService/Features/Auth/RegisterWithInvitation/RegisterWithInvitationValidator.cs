using FluentValidation;
using Clovance.ApiService.Infrastructure.Validation;

namespace Clovance.ApiService.Features.Auth.RegisterWithInvitation;

public sealed class RegisterWithInvitationValidator : AbstractValidator<RegisterWithInvitationCommand>
{
    public RegisterWithInvitationValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("A valid email address is required.");

        RuleFor(x => x.Token)
            .NotEmpty()
            .WithMessage("Invitation token is required.");

        RuleFor(x => x.Password)
            .ApplyPasswordPolicy();
    }
}
