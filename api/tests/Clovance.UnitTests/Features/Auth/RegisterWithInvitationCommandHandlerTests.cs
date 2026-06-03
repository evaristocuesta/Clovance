using Clovance.ApiService.Domain.UserInvitations;
using Clovance.ApiService.Features.Auth.RegisterWithInvitation;
using Clovance.ApiService.Infrastructure.Database;
using Clovance.ApiService.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Clovance.Tests.Features.Auth;

public class RegisterWithInvitationCommandHandlerTests : IDisposable
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ClovanceDbContext _dbContext;
    private readonly IUserInvitationTokenService _tokenService;
    private readonly RegisterWithInvitationCommandHandler _handler;

    public RegisterWithInvitationCommandHandlerTests()
    {
        _userManager = Substitute.For<UserManager<ApplicationUser>>(
            Substitute.For<IUserStore<ApplicationUser>>(),
            null, null, null, null, null, null, null, null);

        var options = new DbContextOptionsBuilder<ClovanceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ClovanceDbContext(options);

        _tokenService = Substitute.For<IUserInvitationTokenService>();

        _handler = new RegisterWithInvitationCommandHandler(
            _userManager,
            _dbContext,
            _tokenService);
    }

    public void Dispose()
    {
        _dbContext?.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task HandleAsync_WithValidInvitation_CreatesUserAndConsumesInvitation()
    {
        var command = new RegisterWithInvitationCommand(
            "newuser@example.com",
            "Password123!",
            "valid-token");

        var tokenHash = "hashed-token";
        
        var invitation = UserInvitation.Create(
            email: "newuser@example.com",
            isAdmin: false,
            tokenHash: tokenHash,
            expiresAt: DateTimeOffset.UtcNow.AddHours(24),
            createdBy: "admin-123"
        );

        _dbContext.UserInvitations.Add(invitation);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var createdUserId = "new-user-123";

        _tokenService.HashToken(command.Token.Trim()).Returns(tokenHash);
        _userManager.FindByEmailAsync(command.Email.Trim()).Returns((ApplicationUser?)null);
        _userManager.CreateAsync(Arg.Any<ApplicationUser>(), command.Password)
            .Returns(callInfo =>
            {
                var user = callInfo.ArgAt<ApplicationUser>(0);
                user.Id = createdUserId;
                return IdentityResult.Success;
            });

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(createdUserId, result.Value.UserId);
        Assert.Equal(command.Email, result.Value.Email);

        var updatedInvitation = await _dbContext.UserInvitations.FindAsync([invitation.Id], CancellationToken.None);
        Assert.NotNull(updatedInvitation);
        Assert.NotNull(updatedInvitation.ConsumedAt);
        Assert.Equal(createdUserId, updatedInvitation.ConsumedBy);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidToken_ReturnsInvitationInvalidOrExpiredError()
    {
        var command = new RegisterWithInvitationCommand(
            "user@example.com",
            "Password123!",
            "invalid-token");

        _tokenService.HashToken(command.Token.Trim()).Returns("wrong-hash");

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorCodes.Auth.InvitationInvalidOrExpired, result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_WithExpiredInvitation_ReturnsInvitationInvalidOrExpiredError()
    {
        var command = new RegisterWithInvitationCommand(
            "user@example.com",
            "Password123!",
            "valid-token");

        var tokenHash = "hashed-token";
        
        var expiredInvitation = UserInvitation.Create(
            email: "user@example.com",
            isAdmin: false,
            tokenHash: tokenHash,
            expiresAt: DateTimeOffset.UtcNow.AddHours(-1),
            createdBy: "admin-123"
        );

        _dbContext.UserInvitations.Add(expiredInvitation);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _tokenService.HashToken(command.Token.Trim()).Returns(tokenHash);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorCodes.Auth.InvitationInvalidOrExpired, result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_WithConsumedInvitation_ReturnsInvitationInvalidOrExpiredError()
    {
        var command = new RegisterWithInvitationCommand(
            "user@example.com",
            "Password123!",
            "valid-token");

        var tokenHash = "hashed-token";

        var consumedInvitation = UserInvitation.Create(
            email: "user@example.com",
            isAdmin: false,
            tokenHash: tokenHash,
            expiresAt: DateTimeOffset.UtcNow.AddHours(24),
            createdBy: "admin-123"
        );

        consumedInvitation.Consume("user-123");

        _dbContext.UserInvitations.Add(consumedInvitation);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _tokenService.HashToken(command.Token.Trim()).Returns(tokenHash);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorCodes.Auth.InvitationInvalidOrExpired, result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_WithExistingUser_ReturnsUserAlreadyExistsError()
    {
        var command = new RegisterWithInvitationCommand(
            "existing@example.com",
            "Password123!",
            "valid-token");

        var tokenHash = "hashed-token";
        
        var invitation = UserInvitation.Create(
            email: "existing@example.com",
            isAdmin: false,
            tokenHash: tokenHash,
            expiresAt: DateTimeOffset.UtcNow.AddHours(24),
            createdBy: "admin-123"
        );

        _dbContext.UserInvitations.Add(invitation);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        var existingUser = new ApplicationUser
        {
            UserName = "existing@example.com",
            Email = "existing@example.com"
        };

        _tokenService.HashToken(command.Token.Trim()).Returns(tokenHash);
        _userManager.FindByEmailAsync(command.Email.Trim()).Returns(existingUser);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorCodes.Auth.UserAlreadyExists, result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_WithUserCreationFailure_ReturnsUserCreationFailedError()
    {
        var command = new RegisterWithInvitationCommand(
            "newuser@example.com",
            "weak",
            "valid-token");

        var tokenHash = "hashed-token";
        
        var invitation = UserInvitation.Create(
            email: "newuser@example.com",
            isAdmin: false,
            tokenHash: tokenHash,
            expiresAt: DateTimeOffset.UtcNow.AddHours(24),
            createdBy: "admin-123"
        );

        _dbContext.UserInvitations.Add(invitation);
        await _dbContext.SaveChangesAsync(CancellationToken.None);

        _tokenService.HashToken(command.Token.Trim()).Returns(tokenHash);
        _userManager.FindByEmailAsync(command.Email.Trim()).Returns((ApplicationUser?)null);
        _userManager.CreateAsync(Arg.Any<ApplicationUser>(), command.Password)
            .Returns(IdentityResult.Failed(new IdentityError { Description = "Password too weak" }));

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorCodes.Auth.UserCreationFailed, result.Error.Code);
    }
}
