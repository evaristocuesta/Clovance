using Clovance.ApiService.Domain.UserInvitations;
using Clovance.ApiService.Features.Auth.RegisterWithInvitation;
using Clovance.ApiService.Infrastructure.Authentication;
using Clovance.ApiService.Infrastructure.Database;
using Clovance.ApiService.Shared;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

namespace Clovance.UnitTests.Features.Auth;

public class RegisterWithInvitationCommandHandlerTests : IAsyncLifetime
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ClovanceDbContext _dbContext;
    private readonly IJwtTokenService _tokenService;
    private readonly RegisterWithInvitationCommandHandler _handler;

    public RegisterWithInvitationCommandHandlerTests()
    {
        _userManager = Substitute.For<UserManager<ApplicationUser>>(
            Substitute.For<IUserStore<ApplicationUser>>(),
            null, null, null, null, null, null, null, null);

        _dbContext = TestDbContextFactory.CreateInMemoryDbContext();

        _tokenService = Substitute.For<IJwtTokenService>();

        _handler = new RegisterWithInvitationCommandHandler(
            _userManager,
            _dbContext,
            _tokenService);
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

        await _dbContext.UserInvitations.AddAsync(invitation, TestContext.Current.CancellationToken);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

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

        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(createdUserId, result.Value.UserId);
        Assert.Equal(command.Email, result.Value.Email);

        var updatedInvitation = await _dbContext.UserInvitations.FindAsync(new object[] { invitation.Id }, TestContext.Current.CancellationToken);
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

        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

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

        await _dbContext.UserInvitations.AddAsync(expiredInvitation, TestContext.Current.CancellationToken);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _tokenService.HashToken(command.Token.Trim()).Returns(tokenHash);

        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

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

        await _dbContext.UserInvitations.AddAsync(consumedInvitation, TestContext.Current.CancellationToken);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _tokenService.HashToken(command.Token.Trim()).Returns(tokenHash);

        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

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

        await _dbContext.UserInvitations.AddAsync(invitation, TestContext.Current.CancellationToken);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var existingUser = new ApplicationUser
        {
            UserName = "existing@example.com",
            Email = "existing@example.com"
        };

        _tokenService.HashToken(command.Token.Trim()).Returns(tokenHash);
        _userManager.FindByEmailAsync(command.Email.Trim()).Returns(existingUser);

        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

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

        await _dbContext.UserInvitations.AddAsync(invitation, TestContext.Current.CancellationToken);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _tokenService.HashToken(command.Token.Trim()).Returns(tokenHash);
        _userManager.FindByEmailAsync(command.Email.Trim()).Returns((ApplicationUser?)null);
        _userManager.CreateAsync(Arg.Any<ApplicationUser>(), command.Password)
            .Returns(IdentityResult.Failed(new IdentityError { Description = "Password too weak" }));

        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorCodes.Auth.UserCreationFailed, result.Error.Code);
    }
}
