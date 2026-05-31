using Clovance.ApiService.Shared;
using FluentValidation;

namespace Clovance.ApiService.Features.Auth.DeleteUser;

public class DeleteUserValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.Auth.UserIdRequired)
            .Must(id => Guid.TryParse(id, out _))
            .WithErrorCode(ErrorCodes.Auth.UserIdInvalid);
    }
}
