using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Shared;
using FluentValidation;

namespace Clovance.ApiService.Features.Accounts.CreateAccount;

public class CreateAccountValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Accounts.AccountNameRequired)
            .MaximumLength(200)
            .WithErrorCode(ErrorCodes.Accounts.AccountNameMaxLength);

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Accounts.AccountCurrencyInvalid)
            .Must(CurrencyValidator.IsValid)
            .WithErrorCode(ErrorCodes.Accounts.AccountCurrencyInvalid);
    }
}
