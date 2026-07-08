using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Clovance.ApiService.Domain.RefreshTokens;
using Clovance.ApiService.Features.Auth.Refresh;
using Clovance.ApiService.Infrastructure.Authentication;
using Clovance.ApiService.Infrastructure.Database;
using Clovance.ApiService.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Clovance.UnitTests.Features.Auth;

public class RefreshCommandHandlerTests : IAsyncLifetime
{
    private readonly ClovanceDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly HttpContext _httpContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly RefreshCommandHandler _handler;

    public RefreshCommandHandlerTests()
    {
        _dbContext = TestDbContextFactory.CreateInMemoryDbContext();
        _httpContext = new DefaultHttpContext();
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _httpContextAccessor.HttpContext.Returns(_httpContext);

        _userManager = Substitute.For<UserManager<ApplicationUser>>(
            Substitute.For<IUserStore<ApplicationUser>>(),
            null, null, null, null, null, null, null, null);

        _jwtTokenService = Substitute.For<IJwtTokenService>();

        _handler = new RefreshCommandHandler(
            _httpContextAccessor,
            _jwtTokenService,
            _userManager,
            _dbContext);
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
    public async Task HandleAsync_ReturnsSuccess()
    {
        // Arrange
        var user = await _dbContext.Users.AddAsync(new ApplicationUser
        {
            Email = "user@example.com"
        }, TestContext.Current.CancellationToken);

        _userManager.FindByIdAsync(Arg.Any<string>()).Returns(user.Entity);

        await _dbContext.RefreshTokens.AddAsync(
            RefreshToken.Create(user.Entity.Id, "hashedNewRefreshToken", DateTimeOffset.UtcNow.AddDays(7)),
            TestContext.Current.CancellationToken);

        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _httpContext.Request.Headers.Cookie = "refreshToken=token";

        _jwtTokenService
            .GetPrincipalFromExpiredToken(Arg.Any<string>())
            .Returns(new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Entity.Id.ToString())
            })));

        _jwtTokenService
            .GenerateToken(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<IEnumerable<string>>())
            .Returns(("newAccessToken", DateTime.UtcNow.AddMinutes(30)));

        _jwtTokenService
            .GenerateToken()
            .Returns("newRefreshToken");

        _jwtTokenService
            .HashToken(Arg.Any<string>())
            .Returns("hashedNewRefreshToken");

        var command = new RefreshCommand();

        // Act
        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);

        var usedRefreshToken = await _dbContext
            .RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == RefreshTokenToken.Create("hashedNewRefreshToken"), TestContext.Current.CancellationToken);

        Assert.NotNull(usedRefreshToken);
        Assert.True(usedRefreshToken.IsUsed);
    }

    [Fact]
    public async Task HandleAsync_ReturnsFailure_WhenNoRefreshToken()
    {
        // Arrange
        var command = new RefreshCommand();
        // Act
        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);
        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorCodes.Auth.UserNotAuthenticated, result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_ReturnsFailure_WhenInvalidToken()
    {
        // Arrange
        var user = await _dbContext.Users.AddAsync(new ApplicationUser
        {
            Email = "user@example.com"
        }, TestContext.Current.CancellationToken);

        _userManager.FindByIdAsync(Arg.Any<string>()).Returns(user.Entity);

        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _httpContext.Request.Headers.Cookie = "refreshToken=token";

        _jwtTokenService.HashToken(Arg.Any<string>())
            .Returns("hashedToken");

        var command = new RefreshCommand();

        // Act
        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorCodes.Auth.UserNotAuthenticated, result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_ReturnFailure_WhenUsedRefreshToken()
    {
        // Arrange
        var user = await _dbContext.Users.AddAsync(new ApplicationUser
        {
            Email = "user@example.com"
        }, TestContext.Current.CancellationToken);

        _userManager.FindByIdAsync(Arg.Any<string>()).Returns(user.Entity);

        var refreshToken = await _dbContext.RefreshTokens.AddAsync(
            RefreshToken.Create(user.Entity.Id, "token", DateTimeOffset.UtcNow.AddDays(7)),
            TestContext.Current.CancellationToken);

        refreshToken.Entity.MarkAsUsed();

        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _httpContext.Request.Headers.Cookie = "refreshToken=token";

        _jwtTokenService.HashToken(Arg.Any<string>())
            .Returns("hashedToken");

        var command = new RefreshCommand();

        // Act
        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorCodes.Auth.UserNotAuthenticated, result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_ReturnFailure_WhenExpiredRefreshToken()
    {
        // Arrange
        var user = await _dbContext.Users.AddAsync(new ApplicationUser
        {
            Email = "user@example.com"
        }, TestContext.Current.CancellationToken);

        _userManager.FindByIdAsync(Arg.Any<string>()).Returns(user.Entity);

        var refreshToken = await _dbContext.RefreshTokens.AddAsync(
            RefreshToken.Create(user.Entity.Id, "hashedToken", DateTimeOffset.UtcNow.AddDays(-7)),
            TestContext.Current.CancellationToken);

        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _httpContext.Request.Headers.Cookie = "refreshToken=token";

        _jwtTokenService.HashToken(Arg.Any<string>())
            .Returns("hashedToken");

        var command = new RefreshCommand();

        // Act
        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorCodes.Auth.UserNotAuthenticated, result.Error.Code);
    }
}
