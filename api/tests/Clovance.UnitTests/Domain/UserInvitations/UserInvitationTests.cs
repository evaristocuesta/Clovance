using Clovance.ApiService.Domain.UserInvitations;
using Clovance.UnitTests.Domain.Shared;

namespace Clovance.UnitTests.Domain.UserInvitations;

public class UserInvitationTests
{
    [Fact]
    public void Create_WithValidUserInvitation_ReturnsUserInvitation()
    {
        var userId = Guid.CreateVersion7();
        var userInvitation = UserInvitation.Create("valid-email@example.com", true, TestData.TokenHash, DateTimeOffset.UtcNow, userId);
        Assert.Equal("valid-email@example.com", userInvitation.Email.Value);
        Assert.Equal(TestData.TokenHash, userInvitation.TokenHash.Value);
    }

    [Fact]
    public void Create_WithInvalidUserInvitation_ThrowsArgumentException()
    {
        var userId = Guid.CreateVersion7();
        Assert.Throws<ArgumentException>(() => UserInvitation.Create("", true, TestData.TokenHash, DateTimeOffset.UtcNow, userId));
    }
}
