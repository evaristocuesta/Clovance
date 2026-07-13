
using Clovance.ApiService.Shared;
using FluentValidation;

namespace Clovance.ApiService.Features.Transactions.GetTransactions;

public sealed class GetTransactionsValidator : AbstractValidator<GetTransactionsQuery>
{
    public GetTransactionsValidator()
    {
        RuleFor(x => x)
            .Must(x => x.DateFrom.HasValue == x.DateTo.HasValue)
            .WithErrorCode(ErrorCodes.Transactions.DateFromDateToMustComeTogether)
            .When(x => x.DateFrom.HasValue || x.DateTo.HasValue);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 200)
            .WithErrorCode(ErrorCodes.Transactions.PageSizeInvalidRange);

        RuleFor(x => x)
            .Must(x => x.CursorDate.HasValue == x.CursorId.HasValue)
            .WithErrorCode(ErrorCodes.Transactions.CursorMustComeTogether)
            .When(x => x.CursorDate.HasValue || x.CursorId.HasValue);
    }
}
