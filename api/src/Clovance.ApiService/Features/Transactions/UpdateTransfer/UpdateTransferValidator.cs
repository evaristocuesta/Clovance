using Clovance.ApiService.Shared;
using FluentValidation;

namespace Clovance.ApiService.Features.Transactions.UpdateTransfer;

public class UpdateTransferValidator : AbstractValidator<UpdateTransferCommand>
{
    public UpdateTransferValidator()
    {
        RuleFor(x => x.TrasactionId)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Transactions.TransactionIdRequired);

        RuleFor(x => x.Date)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Transactions.DateRequired);

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Transactions.DescriptionRequired)
            .MaximumLength(250)
            .WithErrorCode(ErrorCodes.Transactions.DescriptionMaxLength);

        RuleFor(x => x.Amount)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Transactions.AmountRequired)
            .Must(amount => amount > 0)
            .WithErrorCode(ErrorCodes.Transactions.AmountInvalid);

        RuleFor(x => x.FromAccountId)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Accounts.AccountIdRequired);

        RuleFor(x => x.ToAccountId)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Accounts.AccountIdRequired);

        RuleFor(x => x)
            .Must(x => x.ToAccountId != x.FromAccountId)
            .WithErrorCode(ErrorCodes.Transactions.AccountsMustBeDifferent);
    }
}
