using Clovance.ApiService.Infrastructure.Validation;
using Clovance.ApiService.Shared;
using FluentValidation;

namespace Clovance.ApiService.Features.Auth.RegisterAdmin;

public sealed class SetupValidator : AbstractValidator<SetupCommand>
{
    public SetupValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Auth.EmailRequired)
            .EmailAddress()
            .WithErrorCode(ErrorCodes.Auth.EmailInvalid);

        RuleFor(x => x.Password)
            .ApplyPasswordPolicy();
    }
}
