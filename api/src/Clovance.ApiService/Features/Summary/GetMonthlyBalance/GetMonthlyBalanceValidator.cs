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
    }
}
