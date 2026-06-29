using Clovance.ApiService.Shared;
using FluentValidation;

namespace Clovance.ApiService.Features.Auth.UpdateUser;

public sealed class UpdateUserValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Auth.EmailRequired)
            .EmailAddress()
            .WithErrorCode(ErrorCodes.Auth.EmailInvalid);
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
