using Clovance.ApiService.Domain.UserInvitations;

namespace Clovance.UnitTests.Domain.UserInvitations;

public class UserInvitationEmailTests
{
    [Fact]
    public void Create_WithValidEmail_ReturnsUserInvitationEmail()
    {
        var email = "test@example.com";
        var userInvitationEmail = UserInvitationEmail.Create(email);

        Assert.Equal(email, userInvitationEmail.Value);
    }

    [Fact]
    public void Create_WithInvalidEmail_ThrowsArgumentException()
    {
        var invalidEmail = "invalid-email";

        Assert.Throws<ArgumentException>(() => UserInvitationEmail.Create(invalidEmail));
    }

    [Fact]
    public void Create_WithEmptyEmail_ThrowsArgumentException()
    {
        var emptyEmail = "";

        Assert.Throws<ArgumentException>(() => UserInvitationEmail.Create(emptyEmail));
    }

    [Fact]
    public void Create_TrimsEmail()
    {
        var emailWithWhitespace = "  test@example.com  ";
        var userInvitationEmail = UserInvitationEmail.Create(emailWithWhitespace);

        Assert.Equal("test@example.com", userInvitationEmail.Value);
    }
}
