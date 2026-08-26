using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Shared;
using FluentValidation;

namespace Clovance.ApiService.Features.Summary.GetDailyBalance;

public class GetDailyBalanceValidator : AbstractValidator<GetDailyBalanceQuery>
{
    public GetDailyBalanceValidator()
    {
        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12)
            .WithErrorCode(ErrorCodes.Transactions.MonthInvalidRange);

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Accounts.AccountCurrencyInvalid)
            .Must(CurrencyValidator.IsValid)
            .WithErrorCode(ErrorCodes.Accounts.AccountCurrencyInvalid);
    }
}
