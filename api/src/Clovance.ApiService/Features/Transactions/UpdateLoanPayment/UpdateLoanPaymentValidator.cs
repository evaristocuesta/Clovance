using Clovance.ApiService.Shared;
using FluentValidation;

namespace Clovance.ApiService.Features.Transactions.UpdateLoanPayment;

public class UpdateLoanPaymentValidator : AbstractValidator<UpdateLoanPaymentCommand>
{
    public UpdateLoanPaymentValidator()
    {
        RuleFor(x => x.TransactionId)
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
            .GreaterThan(0)
            .WithErrorCode(ErrorCodes.Transactions.AmountInvalid);

        RuleFor(x => x.PrincipalAmount)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Transactions.PrincipalAmountRequired)
            .LessThanOrEqualTo(x => x.Amount)
            .WithErrorCode(ErrorCodes.Transactions.PrincipalAmountMustBeLessThanOrEqualToAmount);

        RuleFor(x => x.FromAccountId)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Accounts.AccountIdRequired);

        RuleFor(x => x.ToAccountId)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Accounts.AccountIdRequired)
            .NotEqual(x => x.FromAccountId)
            .WithErrorCode(ErrorCodes.Transactions.AccountsMustBeDifferent);
    }
}
