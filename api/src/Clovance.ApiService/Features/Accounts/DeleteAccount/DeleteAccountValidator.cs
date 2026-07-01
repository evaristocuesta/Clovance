using Clovance.ApiService.Shared;
using FluentValidation;

namespace Clovance.ApiService.Features.Accounts.DeleteAccount;

public class DeleteAccountValidator : AbstractValidator<DeleteAccountCommand>
{
    public DeleteAccountValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Accounts.AccountIdRequired);
    }
}
