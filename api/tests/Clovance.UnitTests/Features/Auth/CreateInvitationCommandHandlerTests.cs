using System.Security.Claims;
using Clovance.ApiService.Domain.UserInvitations;
using Clovance.ApiService.Features.Auth.CreateInvitation;
using Clovance.ApiService.Infrastructure.Authentication;
using Clovance.ApiService.Infrastructure.Database;
using Clovance.ApiService.Infrastructure.UserInvitations;
using Clovance.ApiService.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Clovance.UnitTests.Features.Auth;

public class CreateInvitationCommandHandlerTests : IAsyncLifetime
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ClovanceDbContext _dbContext;
    private readonly IJwtTokenService _tokenService;
    private readonly IOptions<UserInvitationOptions> _invitationOptions;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly CreateInvitationCommandHandler _handler;
    private readonly HttpContext _httpContext;

    public CreateInvitationCommandHandlerTests()
    {
        _userManager = Substitute.For<UserManager<ApplicationUser>>(
            Substitute.For<IUserStore<ApplicationUser>>(),
            null, null, null, null, null, null, null, null);

        _dbContext = TestDbContextFactory.CreateInMemoryDbContext();

        _tokenService = Substitute.For<IJwtTokenService>();
        _invitationOptions = Substitute.For<IOptions<UserInvitationOptions>>();
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _httpContext = Substitute.For<HttpContext>();

        _invitationOptions.Value.Returns(new UserInvitationOptions { ExpirationHours = 48 });
        _httpContextAccessor.HttpContext.Returns(_httpContext);

        _handler = new CreateInvitationCommandHandler(
            _userManager,
            _dbContext,
            _tokenService,
            _invitationOptions,
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
        var adminUserId = "admin-123";
        var rawToken = "raw-token-123";
        var tokenHash = "hashed-token-123";

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, adminUserId)
        }));

        _httpContext.User.Returns(claimsPrincipal);
        _userManager.GetUserId(claimsPrincipal).Returns(adminUserId);
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
        var adminUserId = "admin-123";
        var existingUser = new ApplicationUser
        {
            Id = "existing-user",
            Email = "existing@example.com"
        };

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, adminUserId)
        }));

        _httpContext.User.Returns(claimsPrincipal);
        _userManager.GetUserId(claimsPrincipal).Returns(adminUserId);
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
        var adminUserId = "admin-123";

        var activeInvitation = UserInvitation.Create(
            email: "invited@example.com",
            isAdmin: false,
            tokenHash: "hashed-token-123",
            expiresAt: DateTimeOffset.UtcNow.AddHours(24),
            createdBy: adminUserId
        );

        await _dbContext.UserInvitations.AddAsync(activeInvitation, TestContext.Current.CancellationToken);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, adminUserId)
        }));

        _httpContext.User.Returns(claimsPrincipal);
        _userManager.GetUserId(claimsPrincipal).Returns(adminUserId);
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
        var adminUserId = "admin-123";

        var expiredInvitation = UserInvitation.Create(
            email: "user@example.com",
            isAdmin: false,
            tokenHash: "hashed-token-123",
            expiresAt: DateTimeOffset.UtcNow.AddHours(-1),
            createdBy: adminUserId
        );


        await _dbContext.UserInvitations.AddAsync(expiredInvitation, TestContext.Current.CancellationToken);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var rawToken = "raw-token-123";
        var tokenHash = "hashed-token-123";

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, adminUserId)
        }));

        _httpContext.User.Returns(claimsPrincipal);
        _userManager.GetUserId(claimsPrincipal).Returns(adminUserId);
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
        var adminUserId = "admin-123";
        var rawToken = "raw-token-123";
        var tokenHash = "hashed-token-123";
        var expirationHours = 72;

        _invitationOptions.Value.Returns(new UserInvitationOptions { ExpirationHours = expirationHours });

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, adminUserId)
        }));

        _httpContext.User.Returns(claimsPrincipal);
        _userManager.GetUserId(claimsPrincipal).Returns(adminUserId);
        _userManager.FindByEmailAsync(command.Email.Trim()).Returns((ApplicationUser?)null);
        _tokenService.GenerateToken().Returns(rawToken);
        _tokenService.HashToken(rawToken).Returns(tokenHash);

        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        var expectedExpiration = DateTimeOffset.UtcNow.AddHours(expirationHours);
        Assert.True(Math.Abs((result.Value.ExpiresAt - expectedExpiration).TotalSeconds) < 2);
    }
}
