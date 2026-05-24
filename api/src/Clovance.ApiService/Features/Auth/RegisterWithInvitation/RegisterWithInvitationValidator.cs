using FluentValidation;

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
            .NotEmpty()
            .MinimumLength(6)
            .WithMessage("Password must be at least 6 characters long.");
    }
}
