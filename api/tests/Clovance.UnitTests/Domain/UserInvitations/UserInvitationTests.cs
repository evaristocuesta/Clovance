using Clovance.ApiService.Domain.UserInvitations;

namespace Clovance.UnitTests.Domain.UserInvitations;

public class UserInvitationTests
{
    [Fact]
    public void Create_WithValidUserInvitation_ReturnsUserInvitation()
    {
        var userId = Guid.CreateVersion7();
        var userInvitation = UserInvitation.Create("valid-email@example.com", true, "valid-token", DateTimeOffset.UtcNow, userId);
        Assert.Equal("valid-email@example.com", userInvitation.Email.Value);
        Assert.Equal("valid-token", userInvitation.TokenHash.Value);
    }

    [Fact]
    public void Create_WithInvalidUserInvitation_ThrowsArgumentException()
    {
        var userId = Guid.CreateVersion7();
        Assert.Throws<ArgumentException>(() => UserInvitation.Create("", true, "valid-token", DateTimeOffset.UtcNow, userId));
    }
}
