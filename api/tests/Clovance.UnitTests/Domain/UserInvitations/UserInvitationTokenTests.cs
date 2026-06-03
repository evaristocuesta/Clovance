using Clovance.ApiService.Domain.UserInvitations;

namespace Clovance.UnitTests.Domain.UserInvitations;

public class UserInvitationTokenTests
{
    [Fact]
    public void Create_WithValidToken_ReturnsUserInvitationToken()
    {
        var userInvitationToken = UserInvitationToken.Create("valid-token");
        Assert.Equal("valid-token", userInvitationToken.Value);
    }

    [Fact]
    public void Create_WithInvalidToken_ThrowsArgumentException()
    {
        var invalidToken = "";

        Assert.Throws<ArgumentException>(() => UserInvitationToken.Create(invalidToken));
    }
}
