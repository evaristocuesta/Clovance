using Clovance.ApiService.Shared;
using FluentValidation;

namespace Clovance.ApiService.Features.Transactions.CreateLoanPayment;

public class CreateLoanPaymentValidator : AbstractValidator<CreateLoanPaymentCommand>
{
    public CreateLoanPaymentValidator()
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
            .GreaterThan(0)
            .WithErrorCode(ErrorCodes.Transactions.AmountInvalid);

        RuleFor(x => x.PrincipalAmount)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Transactions.AmountRequired)
            .LessThanOrEqualTo(x => x.Amount)
            .WithErrorCode(ErrorCodes.Transactions.PrincipalAmountMustBeLessThanOrEqualToAmount);

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
