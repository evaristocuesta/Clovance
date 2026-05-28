using Clovance.ApiService.Features.Auth.CreateInvitation;
using Clovance.ApiService.Infrastructure.Database;
using Clovance.ApiService.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NSubstitute;
using System.Security.Claims;

namespace Clovance.Tests.Features.Auth;

public class CreateInvitationCommandHandlerTests : IDisposable
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ClovanceDbContext _dbContext;
    private readonly IInvitationTokenService _tokenService;
    private readonly IOptions<InvitationOptions> _invitationOptions;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly CreateInvitationCommandHandler _handler;
    private readonly HttpContext _httpContext;

    public CreateInvitationCommandHandlerTests()
    {
        _userManager = Substitute.For<UserManager<ApplicationUser>>(
            Substitute.For<IUserStore<ApplicationUser>>(),
            null, null, null, null, null, null, null, null);

        var options = new DbContextOptionsBuilder<ClovanceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ClovanceDbContext(options);

        _tokenService = Substitute.For<IInvitationTokenService>();
        _invitationOptions = Substitute.For<IOptions<InvitationOptions>>();
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _httpContext = Substitute.For<HttpContext>();

        _invitationOptions.Value.Returns(new InvitationOptions { ExpirationHours = 48 });
        _httpContextAccessor.HttpContext.Returns(_httpContext);

        _handler = new CreateInvitationCommandHandler(
            _userManager,
            _dbContext,
            _tokenService,
            _invitationOptions,
            _httpContextAccessor);
    }

    public void Dispose()
    {
        _dbContext?.Dispose();
        GC.SuppressFinalize(this);
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

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(command.Email, result.Value.Email);
        Assert.Equal(rawToken, result.Value.Token);

        var savedInvitation = await _dbContext.UserInvitations
            .FirstOrDefaultAsync(i => i.Email == command.Email, CancellationToken.None);
        Assert.NotNull(savedInvitation);
        Assert.Equal(tokenHash, savedInvitation.TokenHash);
        Assert.Equal(adminUserId, savedInvitation.CreatedByUserId);
    }

    [Fact]
    public async Task HandleAsync_WithoutAuthentication_ReturnsUserNotAuthenticatedError()
    {
        var command = new CreateInvitationCommand("newuser@example.com");

        _httpContext.User.Returns(new ClaimsPrincipal());
        _userManager.GetUserId(Arg.Any<ClaimsPrincipal>()).Returns((string?)null);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

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

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorCodes.Auth.UserAlreadyExists, result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_WithActiveInvitation_ReturnsActiveInvitationAlreadyExistsError()
    {
        var command = new CreateInvitationCommand("invited@example.com");
        var adminUserId = "admin-123";
        var activeInvitation = new UserInvitation
        {
            Id = Guid.NewGuid(),
            Email = "invited@example.com",
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(24),
            ConsumedAt = null
        };

        _dbContext.UserInvitations.Add(activeInvitation);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, adminUserId)
        }));

        _httpContext.User.Returns(claimsPrincipal);
        _userManager.GetUserId(claimsPrincipal).Returns(adminUserId);
        _userManager.FindByEmailAsync(command.Email.Trim()).Returns((ApplicationUser?)null);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorCodes.Auth.ActiveInvitationAlreadyExists, result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_WithExpiredInvitation_CreatesNewInvitation()
    {
        var command = new CreateInvitationCommand("user@example.com");
        var adminUserId = "admin-123";
        var expiredInvitation = new UserInvitation
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(-1),
            ConsumedAt = null
        };

        _dbContext.UserInvitations.Add(expiredInvitation);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

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

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);

        var invitationCount = await _dbContext.UserInvitations
            .CountAsync(i => i.Email == command.Email, CancellationToken.None);
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

        _invitationOptions.Value.Returns(new InvitationOptions { ExpirationHours = expirationHours });

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, adminUserId)
        }));

        _httpContext.User.Returns(claimsPrincipal);
        _userManager.GetUserId(claimsPrincipal).Returns(adminUserId);
        _userManager.FindByEmailAsync(command.Email.Trim()).Returns((ApplicationUser?)null);
        _tokenService.GenerateToken().Returns(rawToken);
        _tokenService.HashToken(rawToken).Returns(tokenHash);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var expectedExpiration = DateTimeOffset.UtcNow.AddHours(expirationHours);
        Assert.True(Math.Abs((result.Value.ExpiresAt - expectedExpiration).TotalSeconds) < 2);
    }
}
