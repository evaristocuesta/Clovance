using System.Security.Claims;
using Clovance.ApiService.Domain.UserInvitations;
using Clovance.ApiService.Features.Auth.CreateInvitation;
using Clovance.ApiService.Infrastructure.Auth.Token;
using Clovance.ApiService.Infrastructure.Auth.UserInvitation;
using Clovance.ApiService.Infrastructure.Database;
using Clovance.ApiService.Infrastructure.Email;
using Clovance.ApiService.Infrastructure.Frontend;
using Clovance.ApiService.Shared;
using Clovance.UnitTests.Domain.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Clovance.UnitTests.Features.Auth;

public class CreateInvitationCommandHandlerTests : IAsyncLifetime
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ClovanceDbContext _dbContext;
    private readonly ITokenService _tokenService;
    private readonly IEmailSender _emailSender;
    private readonly IStringLocalizer<EmailResources> _localizer;
    private readonly IOptions<UserInvitationOptions> _invitationOptions;
    private readonly IOptions<FrontendOptions> _frontendOptions;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly CreateInvitationCommandHandler _handler;
    private readonly HttpContext _httpContext;

    public CreateInvitationCommandHandlerTests()
    {
        _userManager = Substitute.For<UserManager<ApplicationUser>>(
            Substitute.For<IUserStore<ApplicationUser>>(),
            null, null, null, null, null, null, null, null);

        _dbContext = TestDbContextFactory.CreateInMemoryDbContext();

        _tokenService = Substitute.For<ITokenService>();
        _emailSender = Substitute.For<IEmailSender>();
        _localizer = Substitute.For<IStringLocalizer<EmailResources>>();
        _invitationOptions = Substitute.For<IOptions<UserInvitationOptions>>();
        _frontendOptions = Substitute.For<IOptions<FrontendOptions>>();
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _httpContext = Substitute.For<HttpContext>();

        _invitationOptions.Value.Returns(new UserInvitationOptions { ExpirationHours = 48 });
        _frontendOptions.Value.Returns(new FrontendOptions { BaseUrl = "https://example.com" });
        _httpContextAccessor.HttpContext.Returns(_httpContext);
        _emailSender.IsConfigured.Returns(true);

        _handler = new CreateInvitationCommandHandler(
            _userManager,
            _dbContext,
            _tokenService,
            _emailSender,
            _localizer,
            _invitationOptions,
            _frontendOptions,
            _httpContextAccessor);
    }

    public ValueTask InitializeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await _dbContext.DisposeAsync();
    }

    [Fact]
    public async Task HandleAsync_WithValidData_CreatesInvitation()
    {
        var command = new CreateInvitationCommand("newuser@example.com");
        var adminUserId = Guid.CreateVersion7();
        var rawToken = TestData.PlainToken;
        var tokenHash = TestData.TokenHash;

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, adminUserId.ToString())
        }));

        _httpContext.User.Returns(claimsPrincipal);
        _userManager.GetUserId(claimsPrincipal).Returns(adminUserId.ToString());
        _userManager.FindByEmailAsync(command.Email.Trim()).Returns((ApplicationUser?)null);
        _tokenService.GenerateToken().Returns(rawToken);
        _tokenService.HashToken(rawToken).Returns(tokenHash);

        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(command.Email, result.Value.Email);
        Assert.Equal(rawToken, result.Value.Token);

        var savedInvitation = await _dbContext.UserInvitations
            .FirstOrDefaultAsync(i => i.Email.Value == command.Email, TestContext.Current.CancellationToken);

        Assert.NotNull(savedInvitation);
        Assert.Equal(tokenHash, savedInvitation.TokenHash.Value);
        Assert.Equal(adminUserId, savedInvitation.CreatedBy);
    }

    [Fact]
    public async Task HandleAsync_WithoutAuthentication_ReturnsUserNotAuthenticatedError()
    {
        var command = new CreateInvitationCommand("newuser@example.com");

        _httpContext.User.Returns(new ClaimsPrincipal());
        _userManager.GetUserId(Arg.Any<ClaimsPrincipal>()).Returns((string?)null);

        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorCodes.Auth.UserNotAuthenticated, result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_WithExistingUser_ReturnsUserAlreadyExistsError()
    {
        var command = new CreateInvitationCommand("existing@example.com");
        var adminUserId = Guid.NewGuid();
        var existingUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "existing@example.com"
        };

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, adminUserId.ToString())
        }));

        _httpContext.User.Returns(claimsPrincipal);
        _userManager.GetUserId(claimsPrincipal).Returns(adminUserId.ToString());
        _userManager.FindByEmailAsync(command.Email.Trim()).Returns(existingUser);

        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorCodes.Auth.UserAlreadyExists, result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_WithActiveInvitation_ReturnsActiveInvitationAlreadyExistsError()
    {
        var command = new CreateInvitationCommand("invited@example.com");
        var adminUserId = Guid.CreateVersion7();

        var activeInvitation = UserInvitation.Create(
            email: "invited@example.com",
            isAdmin: false,
            tokenHash: TestData.TokenHash,
            expiresAt: DateTimeOffset.UtcNow.AddHours(24),
            createdBy: adminUserId
        );

        await _dbContext.UserInvitations.AddAsync(activeInvitation, TestContext.Current.CancellationToken);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, adminUserId.ToString())
        }));

        _httpContext.User.Returns(claimsPrincipal);
        _userManager.GetUserId(claimsPrincipal).Returns(adminUserId.ToString());
        _userManager.FindByEmailAsync(command.Email.Trim()).Returns((ApplicationUser?)null);

        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorCodes.Auth.ActiveInvitationAlreadyExists, result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_WithExpiredInvitation_CreatesNewInvitation()
    {
        var command = new CreateInvitationCommand("user@example.com");
        var adminUserId = Guid.CreateVersion7();

        var expiredInvitation = UserInvitation.Create(
            email: "user@example.com",
            isAdmin: false,
            tokenHash: TestData.TokenHash,
            expiresAt: DateTimeOffset.UtcNow.AddHours(-1),
            createdBy: adminUserId
        );


        await _dbContext.UserInvitations.AddAsync(expiredInvitation, TestContext.Current.CancellationToken);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var rawToken = TestData.PlainToken;
        var tokenHash = TestData.TokenHash;

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, adminUserId.ToString())
        }));

        _httpContext.User.Returns(claimsPrincipal);
        _userManager.GetUserId(claimsPrincipal).Returns(adminUserId.ToString());
        _userManager.FindByEmailAsync(command.Email.Trim()).Returns((ApplicationUser?)null);
        _tokenService.GenerateToken().Returns(rawToken);
        _tokenService.HashToken(rawToken).Returns(tokenHash);

        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);

        var invitationCount = await _dbContext.UserInvitations
            .CountAsync(i => i.Email.Value == command.Email, TestContext.Current.CancellationToken);

        Assert.Equal(2, invitationCount);
    }

    [Fact]
    public async Task HandleAsync_SetsCorrectExpirationTime()
    {
        var command = new CreateInvitationCommand("newuser@example.com");
        var adminUserId = Guid.CreateVersion7();
        var rawToken = TestData.PlainToken;
        var tokenHash = TestData.TokenHash;
        var expirationHours = 72;

        _invitationOptions.Value.Returns(new UserInvitationOptions { ExpirationHours = expirationHours });

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, adminUserId.ToString())
        }));

        _httpContext.User.Returns(claimsPrincipal);
        _userManager.GetUserId(claimsPrincipal).Returns(adminUserId.ToString());
        _userManager.FindByEmailAsync(command.Email.Trim()).Returns((ApplicationUser?)null);
        _tokenService.GenerateToken().Returns(rawToken);
        _tokenService.HashToken(rawToken).Returns(tokenHash);

        var handler = new CreateInvitationCommandHandler(
            _userManager,
            _dbContext,
            _tokenService,
            _emailSender,
            _localizer,
            _invitationOptions,
            _frontendOptions,
            _httpContextAccessor);

        var result = await handler.HandleAsync(command, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        var expectedExpiration = DateTimeOffset.UtcNow.AddHours(expirationHours);
        Assert.True(Math.Abs((result.Value.ExpiresAt - expectedExpiration).TotalSeconds) < 2);
    }
}
