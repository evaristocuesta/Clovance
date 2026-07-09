using Clovance.ApiService.Domain.Transactions;
using Clovance.ApiService.Shared;
using FluentValidation;

namespace Clovance.ApiService.Features.Transactions.UpdateTransaction;

public class UpdateTransactionValidator : AbstractValidator<UpdateTransactionCommand>
{
    public UpdateTransactionValidator()
    {
        RuleFor(x => x.Transaction.Id)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Transactions.TransactionIdRequired);

        RuleFor(x => x.Transaction.Date)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Transactions.DateRequired);

        RuleFor(x => x.Transaction.Description)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Transactions.DescriptionRequired)
            .MaximumLength(250)
            .WithErrorCode(ErrorCodes.Transactions.DescriptionMaxLength);

        RuleFor(x => x.Transaction.Amount)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Transactions.AmountRequired)
            .Must(amount => amount != 0)
            .WithErrorCode(ErrorCodes.Transactions.AmountInvalid);

        RuleFor(x => x.Transaction.Type)
            .IsInEnum()
            .WithErrorCode(ErrorCodes.Transactions.TypeInvalid);

        RuleFor(x => x.Transaction.AccountId)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Accounts.AccountIdRequired);

        RuleFor(x => x.Transaction)
            .Must(dto => TransactionAmountTypeRules.EnsureAmountMatchesType(dto.Amount, dto.Type))
            .WithErrorCode(ErrorCodes.Transactions.AmountSignTypeMismatch)
            .When(x => x.Transaction.Amount != 0 && Enum.IsDefined(typeof(TransactionType), x.Transaction.Type));
    }
}
