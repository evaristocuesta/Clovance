using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace Clovance.UnitTests.Features;

public abstract class FeatureTestBase
{
    protected static HttpContext CreateHttpContext(Guid userId)
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }, "TestAuth");

        return new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity)
        };
    }

    protected static void Authenticate(IHttpContextAccessor httpContextAccessor, Guid userId)
    {
        httpContextAccessor.HttpContext.Returns(CreateHttpContext(userId));
    }
}
