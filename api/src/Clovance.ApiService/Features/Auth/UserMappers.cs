using Clovance.ApiService.Infrastructure.Database;

namespace Clovance.ApiService.Features.Auth;

public static class UserMappers
{
    public static UserDto ToDto(
        this ApplicationUser user, 
        IEnumerable<string> roles)
    {
        return new(
            user.Id,
            user.Email!,
            user.MustCompleteOnboarding,
            roles);
    }
}
