using Clovance.ApiService.Domain.UserInvitations;
using Clovance.UnitTests.Domain.Shared;

namespace Clovance.UnitTests.Domain.UserInvitations;

public class UserInvitationTokenTests
{
    [Fact]
    public void Create_WithValidToken_ReturnsUserInvitationToken()
    {
        var userInvitationToken = UserInvitationTokenHash.Create(TestData.TokenHash);
        Assert.Equal(TestData.TokenHash, userInvitationToken.Value);
    }

    [Fact]
    public void Create_WithInvalidToken_ThrowsArgumentException()
    {
        var invalidToken = "";

        Assert.Throws<ArgumentException>(() => UserInvitationTokenHash.Create(invalidToken));
    }
}
