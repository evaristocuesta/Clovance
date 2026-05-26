using FluentValidation;

namespace Clovance.ApiService.Infrastructure.Validation;

public static class PasswordValidationExtensions
{
    public static IRuleBuilderOptions<T, string> ApplyPasswordPolicy<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .MinimumLength(12)
            .WithMessage("New password must be at least 12 characters long.")
            .Matches("[0-9]")
            .WithMessage("New password must contain at least one digit.")
            .Matches("[a-z]")
            .WithMessage("New password must contain at least one lowercase letter.")
            .Matches("[A-Z]")
            .WithMessage("New password must contain at least one uppercase letter.")
            .Matches("[^a-zA-Z0-9]")
            .WithMessage("New password must contain at least one non-alphanumeric character.");
    }
}
