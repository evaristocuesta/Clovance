using Clovance.ApiService.Shared;
using FluentValidation;

namespace Clovance.ApiService.Features.Auth.CreateInvitation;

public sealed class CreateInvitationValidator : AbstractValidator<CreateInvitationCommand>
{
    public CreateInvitationValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Auth.EmailRequired)
            .EmailAddress()
            .WithErrorCode(ErrorCodes.Auth.EmailInvalid);
    }
}
