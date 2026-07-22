using Clovance.ApiService.Shared;
using FluentValidation;

namespace Clovance.ApiService.Features.Summary.GetMonthlyCashflow;

public class GetMonthlyCashflowValidator : AbstractValidator<GetMonthlyCashflowQuery>
{
    public GetMonthlyCashflowValidator()
    {
        RuleFor(x => x.AnchorMonth)
            .InclusiveBetween(1, 12)
            .WithErrorCode(ErrorCodes.Transactions.MonthInvalidRange);
    }
}
