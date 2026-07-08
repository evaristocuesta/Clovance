using Clovance.ApiService.Shared;
using FluentValidation;

namespace Clovance.ApiService.Features.Transactions.GetTransactionById;

public class GetTransactionByIdValidator : AbstractValidator<GetTransactionByIdQuery>
{
    public GetTransactionByIdValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Transactions.TransactionIdRequired);
    }
}
