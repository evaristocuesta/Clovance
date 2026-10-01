using Clovance.ApiService.Infrastructure.Database;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

namespace Clovance.UnitTests.Features.Auth;

public abstract class AuthHandlerTestBase : FeatureTestBase
{
    protected static UserManager<ApplicationUser> CreateUserManager()
    {
        return Substitute.For<UserManager<ApplicationUser>>(
            Substitute.For<IUserStore<ApplicationUser>>(),
            null, null, null, null, null, null, null, null);
    }
}
