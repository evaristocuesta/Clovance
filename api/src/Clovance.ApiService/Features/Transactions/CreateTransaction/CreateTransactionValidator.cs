using Clovance.ApiService.Shared;
using FluentValidation;

namespace Clovance.ApiService.Features.Transactions.CreateTransaction;

public class CreateTransactionValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionValidator()
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
            .Must(amount => amount != 0)
            .WithErrorCode(ErrorCodes.Transactions.AmountInvalid);
        
        RuleFor(x => x.AccountId)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Accounts.AccountIdRequired);
    }
}
