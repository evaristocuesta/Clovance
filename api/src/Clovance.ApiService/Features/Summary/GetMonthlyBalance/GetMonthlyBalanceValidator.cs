using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Shared;
using FluentValidation;

namespace Clovance.ApiService.Features.Summary.GetMonthlyBalance;

public class GetMonthlyBalanceValidator : AbstractValidator<GetMonthlyBalanceQuery>
{
    public GetMonthlyBalanceValidator()
    {
        RuleFor(x => x.AnchorMonth)
            .InclusiveBetween(1, 12)
            .WithErrorCode(ErrorCodes.Transactions.MonthInvalidRange);

        RuleFor(x => x.MonthsBack)
            .GreaterThan(0)
            .LessThanOrEqualTo(120);

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Accounts.AccountCurrencyInvalid)
            .Must(CurrencyValidator.IsValid)
            .WithErrorCode(ErrorCodes.Accounts.AccountCurrencyInvalid);
    }
}
