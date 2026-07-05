using Clovance.ApiService.Shared;
using FluentValidation;

namespace Clovance.ApiService.Features.Accounts.RestoreAccount;

public class RestoreAccountValidator : AbstractValidator<RestoreAccountCommand>
{
    public RestoreAccountValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Accounts.AccountIdRequired);
    }
}
