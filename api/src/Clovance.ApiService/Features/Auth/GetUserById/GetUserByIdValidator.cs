using Clovance.ApiService.Shared;
using FluentValidation;

namespace Clovance.ApiService.Features.Auth.GetUserById;

public sealed class GetUserByIdValidator : AbstractValidator<GetUserByIdRequest>
{
    public GetUserByIdValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Auth.UserIdRequired)
            .Must(id => Guid.TryParse(id, out _))
            .WithErrorCode(ErrorCodes.Auth.UserIdInvalid);

    }
}
