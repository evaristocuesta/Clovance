using Clovance.ApiService.Domain.Accounts;
using Clovance.ApiService.Features.Summary.Shared;
using Clovance.ApiService.Shared;
using FluentValidation;

namespace Clovance.ApiService.Features.Summary.GetDailyCashflow;

public class GetDailyCashflowValidator : AbstractValidator<GetDailyCashflowQuery>
{
    public GetDailyCashflowValidator()
    {
        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12)
            .WithErrorCode(ErrorCodes.Transactions.MonthInvalidRange);

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Accounts.AccountCurrencyInvalid)
            .Must(CurrencyValidator.IsValid)
            .WithErrorCode(ErrorCodes.Accounts.AccountCurrencyInvalid);

        RuleFor(x => x)
            .Must(x => x.AccountType is null || Enum.IsDefined(typeof(AccountTypeFilter), x.AccountType))
            .WithErrorCode(ErrorCodes.Accounts.AccountTypeInvalid)
            .Must(x => x.AccountId is null ? x.AccountType.HasValue : !x.AccountType.HasValue)
            .WithErrorCode(ErrorCodes.Accounts.AccountTypeInvalid);
    }
}
