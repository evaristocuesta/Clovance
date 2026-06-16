using Clovance.ApiService.Features.Shared;
using Clovance.ApiService.Infrastructure.Database;
using Microsoft.AspNetCore.Identity;

namespace Clovance.ApiService.Features.Auth.GetUsers;

public class GetUsersQueryHandler : IHandler<GetUsersQuery, Result<GetUsersResult>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    public GetUsersQueryHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<GetUsersResult>> HandleAsync(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = _userManager.Users.ToList();
        var userDtos = new List<UserDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userDtos.Add(user.ToDto(roles));
        }

        return Result<GetUsersResult>.Success(new GetUsersResult(userDtos));
    }
}
