using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Shared;
using FluentValidation;

namespace Clovance.ApiService.Features.Accounts.UpdateAccount;

public class UpdateAccountValidator : AbstractValidator<UpdateAccountCommand>
{
    public UpdateAccountValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Accounts.AccountIdRequired);

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
