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
    }
}
