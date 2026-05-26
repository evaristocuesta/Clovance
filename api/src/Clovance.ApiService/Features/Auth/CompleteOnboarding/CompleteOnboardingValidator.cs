using FluentValidation;
using Clovance.ApiService.Infrastructure.Validation;

namespace Clovance.ApiService.Features.Auth.CompleteOnboarding;

public sealed class CompleteOnboardingValidator : AbstractValidator<CompleteOnboardingCommand>
{
    public CompleteOnboardingValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
            .WithMessage("Current password is required.");

        RuleFor(x => x.NewPassword)
            .ApplyPasswordPolicy();

        RuleFor(x => x.NewEmail)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("A valid email address is required.");
    }
}
