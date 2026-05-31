using Clovance.ApiService.Shared;
using FluentValidation;

namespace Clovance.ApiService.Infrastructure.Validation;

public static class PasswordValidationExtensions
{
    public static IRuleBuilderOptions<T, string> ApplyPasswordPolicy<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Auth.PasswordRequired)
            .MinimumLength(12)
            .WithErrorCode(ErrorCodes.Auth.PasswordMinLength)
            .Matches("[0-9]")
            .WithErrorCode(ErrorCodes.Auth.PasswordMissingDigit)
            .Matches("[a-z]")
            .WithErrorCode(ErrorCodes.Auth.PasswordMissingLowercase)
            .Matches("[A-Z]")
            .WithErrorCode(ErrorCodes.Auth.PasswordMissingUppercase)
            .Matches("[^a-zA-Z0-9]")
            .WithErrorCode(ErrorCodes.Auth.PasswordMissingNonAlphanumeric);
    }
}
