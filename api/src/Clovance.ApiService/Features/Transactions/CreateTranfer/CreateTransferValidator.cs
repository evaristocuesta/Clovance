using Clovance.ApiService.Domain.Transactions;
using Clovance.ApiService.Shared;
using FluentValidation;

namespace Clovance.ApiService.Features.Transactions.CreateTranfer;

public class CreateTransferValidator : AbstractValidator<CreateTransferCommand>
{
    public CreateTransferValidator()
    {
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

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithErrorCode(ErrorCodes.Transactions.TypeInvalid)
            .Must(type => type == TransactionType.Transfer)
            .WithErrorCode(ErrorCodes.Transactions.TypeInvalid);

        RuleFor(x => x.FromAccountId)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Accounts.AccountIdRequired);

        RuleFor(x => x.ToAccountId)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Accounts.AccountIdRequired);
    }
}
