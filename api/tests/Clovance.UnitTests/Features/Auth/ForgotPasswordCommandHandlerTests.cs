using Clovance.ApiService.Features.Auth.ForgotPassword;
using Clovance.ApiService.Infrastructure.Database;
using Clovance.ApiService.Infrastructure.Email;
using Clovance.ApiService.Infrastructure.Frontend;
using Clovance.ApiService.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Clovance.UnitTests.Features.Auth;

public class ForgotPasswordCommandHandlerTests : AuthHandlerTestBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly IOptions<FrontendOptions> _frontendOptions;
    private readonly IStringLocalizer<EmailResources> _localizer;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;
    private readonly ForgotPasswordCommandHandler _handler;

    public ForgotPasswordCommandHandlerTests()
    {
        _userManager = CreateUserManager();

        _emailSender = Substitute.For<IEmailSender>();
        _frontendOptions = Substitute.For<IOptions<FrontendOptions>>();
        _localizer = Substitute.For<IStringLocalizer<EmailResources>>();
        _logger = Substitute.For<ILogger<ForgotPasswordCommandHandler>>();

        _frontendOptions.Value.Returns(new FrontendOptions { BaseUrl = "https://example.com" });

        _localizer["PasswordReset_Intro"].Returns(new LocalizedString("PasswordReset_Intro", "Reset your password."));
        _localizer["PasswordReset_LinkText"].Returns(new LocalizedString("PasswordReset_LinkText", "Reset password"));
        _localizer["PasswordReset_Ignore"].Returns(new LocalizedString("PasswordReset_Ignore", "If you did not request this, ignore it."));
        _localizer["PasswordReset_Subject"].Returns(new LocalizedString("PasswordReset_Subject", "Password reset"));

        _handler = new ForgotPasswordCommandHandler(
            _userManager,
            _emailSender,
            _frontendOptions,
            _localizer,
            _logger);
    }

    [Fact]
    public async Task HandleAsync_WhenEmailSenderIsNotConfigured_ReturnsEmailNotConfiguredError()
    {
        // Arrange
        var command = new ForgotPasswordCommand("user@example.com");
        _emailSender.IsConfigured.Returns(false);

        // Act
        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Auth.EmailNotConfigured, result.Error?.Code);
        await _userManager.DidNotReceive().FindByEmailAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserDoesNotExist_ReturnsSuccessWithoutSendingEmail()
    {
        // Arrange
        var command = new ForgotPasswordCommand("missing@example.com");
        _emailSender.IsConfigured.Returns(true);
        _userManager.FindByEmailAsync(command.Email).Returns((ApplicationUser?)null);

        // Act
        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        await _emailSender.DidNotReceive().SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenUserExists_SendsResetEmailWithToken()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Email = "user@example.com",
            UserName = "user@example.com"
        };
        var command = new ForgotPasswordCommand(user.Email);
        var token = "reset-token-123";

        _emailSender.IsConfigured.Returns(true);
        _userManager.FindByEmailAsync(command.Email).Returns(user);
        _userManager.GeneratePasswordResetTokenAsync(user).Returns(token);

        EmailMessage? sentMessage = null;
        _emailSender.SendAsync(Arg.Do<EmailMessage>(message => sentMessage = message), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(sentMessage);
        Assert.Equal(user.Email, sentMessage!.To);
        Assert.Equal("Password reset", sentMessage.Subject);
        Assert.Contains("https://example.com/auth/reset-password?token=reset-token-123", sentMessage.HtmlBody);
        Assert.Contains("user%40example.com", sentMessage.HtmlBody);
    }

    [Fact]
    public async Task HandleAsync_WhenEmailSendFails_ReturnsEmailSendFailedError()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Email = "user@example.com",
            UserName = "user@example.com"
        };
        var command = new ForgotPasswordCommand(user.Email);

        _emailSender.IsConfigured.Returns(true);
        _userManager.FindByEmailAsync(command.Email).Returns(user);
        _userManager.GeneratePasswordResetTokenAsync(user).Returns("reset-token-123");
        _emailSender.SendAsync(Arg.Any<EmailMessage>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new Exception("SMTP unavailable")));

        // Act
        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCodes.Auth.EmailSendFailed, result.Error?.Code);
        Assert.Contains("SMTP unavailable", result.Error?.Description);
    }
}
