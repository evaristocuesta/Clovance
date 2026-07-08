
using Clovance.ApiService.Shared;
using FluentValidation;

namespace Clovance.ApiService.Features.Transactions.GetTransactions;

public sealed class GetTransactionsValidator : AbstractValidator<GetTransactionsQuery>
{
    public GetTransactionsValidator()
    {
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Description) || (x.Year.HasValue && x.Month.HasValue))
            .WithErrorCode(ErrorCodes.Transactions.FilterRequired);

        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12)
            .When(x => x.Month.HasValue)
            .WithErrorCode(ErrorCodes.Transactions.MonthInvalidRange);

        RuleFor(x => x)
            .Must(x => x.Year.HasValue == x.Month.HasValue)
            .WithErrorCode(ErrorCodes.Transactions.YearMonthMustComeTogether)
            .When(x => x.Year.HasValue || x.Month.HasValue);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 200)
            .WithErrorCode(ErrorCodes.Transactions.PageSizeInvalidRange);

        RuleFor(x => x)
            .Must(x => x.CursorDate.HasValue == x.CursorId.HasValue)
            .WithErrorCode(ErrorCodes.Transactions.CursorMustComeTogether);
    }
}
