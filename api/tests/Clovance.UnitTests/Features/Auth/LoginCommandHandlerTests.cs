using Clovance.ApiService.Domain.RefreshTokens;
using Clovance.ApiService.Features.Auth.Login;
using Clovance.ApiService.Infrastructure.Authentication;
using Clovance.ApiService.Infrastructure.Database;
using Clovance.ApiService.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Clovance.UnitTests.Features.Auth;

public class LoginCommandHandlerTests
{
    private readonly ClovanceDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly HttpContext _httpContext;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        // Setup in-memory database for testing
        var options = new DbContextOptionsBuilder<ClovanceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ClovanceDbContext(options);

        _userManager = Substitute.For<UserManager<ApplicationUser>>(
            Substitute.For<IUserStore<ApplicationUser>>(),
            null, null, null, null, null, null, null, null);

        _jwtTokenService = Substitute.For<IJwtTokenService>();
        _httpContext = new DefaultHttpContext();
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _httpContextAccessor.HttpContext.Returns(_httpContext);

        _handler = new LoginCommandHandler(_dbContext, _httpContextAccessor, _userManager, _jwtTokenService);
    }

    [Fact]
    public async Task HandleAsync_WithValidCredentials_ReturnsSuccessWithToken()
    {
        // Arrange
        var command = new LoginCommand("test@example.com", "Password123!");

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "test@example.com",
            Email = "test@example.com",
            MustCompleteOnboarding = false
        };

        var roles = new List<string> { "User" };
        var expectedToken = "jwt-token";
        var expectedRefreshToken = "refresh-token-12345";
        var expectedHashedToken = "hashed-refresh-token";
        var expectedExpiresAt = DateTimeOffset.UtcNow.AddMinutes(60);

        _userManager.FindByEmailAsync(command.Email).Returns(user);
        _userManager.CheckPasswordAsync(user, command.Password).Returns(true);
        _userManager.GetRolesAsync(user).Returns(roles);
        
        _jwtTokenService.GenerateToken(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<string>>(), Arg.Any<bool>())
            .Returns((expectedToken, expectedExpiresAt));
        
        _jwtTokenService.GenerateToken().Returns(expectedRefreshToken);
        _jwtTokenService.HashToken(Arg.Any<string>()).Returns(expectedHashedToken);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedToken, result.Value.AccessToken);
        Assert.Equal(expectedExpiresAt, result.Value.ExpiresAt);

        // Verify refresh token was generated
        _jwtTokenService.Received(1).GenerateToken();

        // Verify refresh token was saved to database
        var savedRefreshToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(TestContext.Current.CancellationToken);
        Assert.NotNull(savedRefreshToken);
        Assert.Equal(user.Id, savedRefreshToken.UserId.Value);
        Assert.Equal(expectedHashedToken, savedRefreshToken.Token.Value);
        Assert.Equal(expectedExpiresAt.AddDays(7), savedRefreshToken.ExpiresAt);

        // Verify cookie was set
        Assert.True(_httpContext.Response.Headers.ContainsKey("Set-Cookie"));
        var cookieHeader = _httpContext.Response.Headers["Set-Cookie"].ToString();
        Assert.Contains("refreshToken=" + expectedRefreshToken, cookieHeader);
        Assert.Contains("httponly", cookieHeader.ToLower());
        Assert.Contains("secure", cookieHeader.ToLower());
        Assert.Contains("samesite=strict", cookieHeader.ToLower());
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentUser_ReturnsInvalidCredentialsError()
    {
        var command = new LoginCommand("nonexistent@example.com", "Password123!");

        _userManager.FindByEmailAsync(command.Email).Returns((ApplicationUser?)null);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorCodes.Auth.InvalidCredentials, result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidPassword_ReturnsInvalidCredentialsError()
    {
        var command = new LoginCommand("test@example.com", "WrongPassword");
        var user = new ApplicationUser
        {
            UserName = "test@example.com",
            Email = "test@example.com"
        };

        _userManager.FindByEmailAsync(command.Email).Returns(user);
        _userManager.CheckPasswordAsync(user, command.Password).Returns(false);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorCodes.Auth.InvalidCredentials, result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_WithMustCompleteOnboarding_IncludesInToken()
    {
        var command = new LoginCommand("test@example.com", "Password123!");

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "test@example.com",
            Email = "test@example.com",
            MustCompleteOnboarding = true
        };
        var roles = new List<string> { "User" };

        _userManager.FindByEmailAsync(command.Email).Returns(user);
        _userManager.CheckPasswordAsync(user, command.Password).Returns(true);
        _userManager.GetRolesAsync(user).Returns(roles);

        _jwtTokenService.GenerateToken(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<string>>(), true)
            .Returns(("token", DateTimeOffset.UtcNow.AddMinutes(60)));

        _jwtTokenService.GenerateToken().Returns("refresh-token");
        _jwtTokenService.HashToken(Arg.Any<string>()).Returns("hashed-refresh-token");

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _jwtTokenService.Received(1).GenerateToken(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<IEnumerable<string>>(),
            true);
    }

    [Fact]
    public async Task HandleAsync_WithValidCredentials_SavesRefreshTokenToDatabase()
    {
        // Arrange
        var command = new LoginCommand("test@example.com", "Password123!");
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "test@example.com",
            Email = "test@example.com",
            MustCompleteOnboarding = false
        };
        var roles = new List<string> { "User" };
        var expectedRefreshToken = "refresh-token-abc123";
        var expectedHashedToken = "hashed-refresh-token";
        var expectedExpiresAt = DateTimeOffset.UtcNow.AddMinutes(60);

        _userManager.FindByEmailAsync(command.Email).Returns(user);
        _userManager.CheckPasswordAsync(user, command.Password).Returns(true);
        _userManager.GetRolesAsync(user).Returns(roles);
        
        _jwtTokenService.GenerateToken(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<string>>(), Arg.Any<bool>())
            .Returns(("jwt-token", expectedExpiresAt));
        
        _jwtTokenService.GenerateToken().Returns(expectedRefreshToken);
        _jwtTokenService.HashToken(Arg.Any<string>()).Returns(expectedHashedToken);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        var savedToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(TestContext.Current.CancellationToken);
        Assert.NotNull(savedToken);
        Assert.Equal(user.Id, savedToken.UserId.Value);
        Assert.Equal(expectedHashedToken, savedToken.Token.Value);
        Assert.True(savedToken.ExpiresAt > DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task HandleAsync_WithValidCredentials_SetsRefreshTokenCookie()
    {
        // Arrange
        var command = new LoginCommand("test@example.com", "Password123!");
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "test@example.com",
            Email = "test@example.com",
            MustCompleteOnboarding = false
        };
        var roles = new List<string> { "User" };
        var expectedRefreshToken = "refresh-token-xyz789";

        _userManager.FindByEmailAsync(command.Email).Returns(user);
        _userManager.CheckPasswordAsync(user, command.Password).Returns(true);
        _userManager.GetRolesAsync(user).Returns(roles);
        
        _jwtTokenService.GenerateToken(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<string>>(), Arg.Any<bool>())
            .Returns(("jwt-token", DateTimeOffset.UtcNow.AddMinutes(60)));
        
        _jwtTokenService.GenerateToken().Returns(expectedRefreshToken);
        _jwtTokenService.HashToken(Arg.Any<string>()).Returns("hashed-refresh-token");

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert - Verify cookie was set with correct properties
        Assert.True(_httpContext.Response.Headers.ContainsKey("Set-Cookie"));
        var cookieHeader = _httpContext.Response.Headers["Set-Cookie"].ToString();
        Assert.Contains("refreshToken=" + expectedRefreshToken, cookieHeader);
        Assert.Contains("httponly", cookieHeader.ToLower());
        Assert.Contains("secure", cookieHeader.ToLower());
        Assert.Contains("samesite=strict", cookieHeader.ToLower());
    }
}
