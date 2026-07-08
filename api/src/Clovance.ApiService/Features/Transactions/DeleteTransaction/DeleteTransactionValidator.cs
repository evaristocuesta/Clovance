using Clovance.ApiService.Shared;
using FluentValidation;

namespace Clovance.ApiService.Features.Transactions.DeleteTransaction;

public class DeleteTransactionValidator : AbstractValidator<DeleteTransactionCommand>
{
    public DeleteTransactionValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Transactions.TransactionIdRequired);
    }
}
