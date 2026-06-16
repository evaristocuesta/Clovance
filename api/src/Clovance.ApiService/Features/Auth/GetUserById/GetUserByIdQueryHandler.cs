using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.Database;
using Microsoft.AspNetCore.Identity;

namespace Clovance.ApiService.Features.Auth.GetUserById;

public class GetUserByIdQueryHandler : IHandler<GetUserByIdQuery, Result<GetUserByIdResult>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public GetUserByIdQueryHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<GetUserByIdResult>> HandleAsync(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = _userManager
            .Users
            .FirstOrDefault(u => u.Id == request.UserId);

        if (user is null)
        {
            return Result<GetUserByIdResult>.Failure(AppErrors.Auth.UserNotFound());
        }

        var roles = await _userManager.GetRolesAsync(user);
        var userDto = user.ToDto(roles);

        return Result<GetUserByIdResult>.Success(new GetUserByIdResult(userDto));
    }
}
