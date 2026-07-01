using Clovance.ApiService.Shared;
using FluentValidation;

namespace Clovance.ApiService.Features.Accounts.GetAccountById;

public class GetAccountByIdValidator : AbstractValidator<GetAccountByIdQuery>
{
    public GetAccountByIdValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage(ErrorCodes.Accounts.AccountIdRequired);
    }
}
