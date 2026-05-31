using Clovance.ApiService.Infrastructure.Validation;
using Clovance.ApiService.Shared;
using FluentValidation;

namespace Clovance.ApiService.Features.Auth.CompleteOnboarding;

public sealed class CompleteOnboardingValidator : AbstractValidator<CompleteOnboardingCommand>
{
    public CompleteOnboardingValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Auth.CurrentPasswordRequired);

        RuleFor(x => x.NewPassword)
            .ApplyPasswordPolicy();

        RuleFor(x => x.NewEmail)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Auth.EmailRequired)
            .EmailAddress()
            .WithErrorCode(ErrorCodes.Auth.EmailInvalid);
    }
}
