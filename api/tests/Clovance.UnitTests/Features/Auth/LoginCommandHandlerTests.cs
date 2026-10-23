using Clovance.ApiService.Features.Auth.Login;
using Clovance.ApiService.Infrastructure.Auth.Jwt;
using Clovance.ApiService.Infrastructure.Auth.Token;
using Clovance.ApiService.Infrastructure.Database;
using Clovance.ApiService.Shared;
using Clovance.UnitTests.Domain.Shared;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Clovance.UnitTests.Features.Auth;

public class LoginCommandHandlerTests : AuthHandlerTestBase, IAsyncLifetime
{
    private readonly ClovanceDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ITokenService _tokenService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly HttpContext _httpContext;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _dbContext = TestDbContextFactory.CreateInMemoryDbContext();

        _userManager = CreateUserManager();
        _signInManager = Substitute.For<SignInManager<ApplicationUser>>(
            _userManager,
            Substitute.For<IHttpContextAccessor>(),
            Substitute.For<IUserClaimsPrincipalFactory<ApplicationUser>>(),
            Substitute.For<IOptions<IdentityOptions>>(),
            Substitute.For<ILogger<SignInManager<ApplicationUser>>>(),
            Substitute.For<IAuthenticationSchemeProvider>(),
            Substitute.For<IUserConfirmation<ApplicationUser>>());

        _jwtTokenService = Substitute.For<IJwtTokenService>();
        _tokenService = Substitute.For<ITokenService>();
        _httpContext = new DefaultHttpContext();
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _httpContextAccessor.HttpContext.Returns(_httpContext);
        
        _handler = new LoginCommandHandler(_dbContext, _httpContextAccessor, _userManager, _signInManager, _jwtTokenService, _tokenService);
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
    public async Task HandleAsync_WithValidCredentials_ReturnsSuccessWithToken()
    {
        // Arrange
        var command = new LoginCommand("test@example.com", "Password123!");

        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            UserName = "test@example.com",
            Email = "test@example.com"
        };

        var roles = new List<string> { "User" };
        var expectedToken = "jwt-token";
        var expectedRefreshToken = TestData.PlainToken;
        var expectedHashedToken = TestData.TokenHash;
        var expectedExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15);

        _userManager.FindByEmailAsync(command.Email).Returns(user);
        _signInManager.CheckPasswordSignInAsync(user, command.Password, true).Returns(SignInResult.Success);
        _userManager.GetRolesAsync(user).Returns(roles);
        _userManager.Users.Returns(new List<ApplicationUser> { user }.AsQueryable());

        _jwtTokenService.GenerateToken(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<IEnumerable<string>>())
            .Returns((expectedToken, expectedExpiresAt));

        _tokenService.GenerateToken().Returns(expectedRefreshToken);
        _tokenService.HashToken(Arg.Any<string>()).Returns(expectedHashedToken);

        // Act
        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedToken, result.Value.AccessToken);
        Assert.Equal(expectedExpiresAt, result.Value.ExpiresAt);

        // Verify refresh token was generated
        _tokenService.Received(1).GenerateToken();

        // Verify refresh token was saved to database
        var savedRefreshToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(TestContext.Current.CancellationToken);
        Assert.NotNull(savedRefreshToken);
        Assert.Equal(user.Id, savedRefreshToken.UserId.Value);
        Assert.Equal(expectedHashedToken, savedRefreshToken.TokenHash.Value);
        Assert.Equal(expectedExpiresAt.AddDays(7), savedRefreshToken.ExpiresAt);

        // Verify cookie was set
        Assert.True(_httpContext.Response.Headers.ContainsKey("Set-Cookie"));
        var cookieHeader = _httpContext.Response.Headers["Set-Cookie"].ToString();
        Assert.Contains("clovance-refresh-token=" + expectedRefreshToken, cookieHeader);
        Assert.Contains("httponly", cookieHeader.ToLower());
        var containsSecure = cookieHeader.ToLower().Contains("secure");
        Assert.Equal(_httpContext.Request.IsHttps, containsSecure);
        Assert.Contains("samesite=strict", cookieHeader.ToLower());
    }

    [Fact]
    public async Task HandleAsync_WithSetupNotCompleted_ReturnsSetupIsNotCompletedError()
    {
        var command = new LoginCommand("nonexistent@example.com", "Password123!");

        _userManager.FindByEmailAsync(command.Email).Returns((ApplicationUser?)null);

        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorCodes.Auth.SetupIsNotCompleted, result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentUser_ReturnsInvalidCredentialsError()
    {
        var command = new LoginCommand("nonexistent@example.com", "Password123!");

        var user = new ApplicationUser
        {
            UserName = "test@example.com",
            Email = "test@example.com"
        };

        _userManager.FindByEmailAsync(command.Email).Returns((ApplicationUser?)null);
        _userManager.Users.Returns(new List<ApplicationUser> { user }.AsQueryable());

        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

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
        _signInManager.CheckPasswordSignInAsync(user, command.Password, true).Returns(SignInResult.Failed);
        _userManager.Users.Returns(new List<ApplicationUser> { user }.AsQueryable());

        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorCodes.Auth.InvalidCredentials, result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_WithValidCredentials_SavesRefreshTokenToDatabase()
    {
        // Arrange
        var command = new LoginCommand("test@example.com", "Password123!");

        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            UserName = "test@example.com",
            Email = "test@example.com"
        };

        var roles = new List<string> { "User" };
        var expectedRefreshToken = TestData.PlainToken;
        var expectedHashedToken = TestData.TokenHash;
        var expectedExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15);

        _userManager.FindByEmailAsync(command.Email).Returns(user);
        _signInManager.CheckPasswordSignInAsync(user, command.Password, true).Returns(SignInResult.Success);
        _userManager.GetRolesAsync(user).Returns(roles);
        _userManager.Users.Returns(new List<ApplicationUser> { user }.AsQueryable());

        _jwtTokenService.GenerateToken(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<IEnumerable<string>>())
            .Returns(("jwt-token", expectedExpiresAt));

        _tokenService.GenerateToken().Returns(expectedRefreshToken);
        _tokenService.HashToken(Arg.Any<string>()).Returns(expectedHashedToken);

        // Act
        await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        var savedToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(TestContext.Current.CancellationToken);
        Assert.NotNull(savedToken);
        Assert.Equal(user.Id, savedToken.UserId.Value);
        Assert.Equal(expectedHashedToken, savedToken.TokenHash.Value);
        Assert.True(savedToken.ExpiresAt > DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task HandleAsync_WithValidCredentials_SetsRefreshTokenCookie()
    {
        // Arrange
        var command = new LoginCommand("test@example.com", "Password123!");

        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            UserName = "test@example.com",
            Email = "test@example.com"
        };

        var roles = new List<string> { "User" };
        var expectedRefreshToken = TestData.PlainToken;

        _userManager.FindByEmailAsync(command.Email).Returns(user);
        _signInManager.CheckPasswordSignInAsync(user, command.Password, true).Returns(SignInResult.Success);
        _userManager.GetRolesAsync(user).Returns(roles);
        _userManager.Users.Returns(new List<ApplicationUser> { user }.AsQueryable());

        _jwtTokenService.GenerateToken(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<IEnumerable<string>>())
            .Returns(("jwt-token", DateTimeOffset.UtcNow.AddMinutes(15)));

        _tokenService.GenerateToken().Returns(expectedRefreshToken);
        _tokenService.HashToken(Arg.Any<string>()).Returns(TestData.TokenHash);

        // Act
        await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert - Verify cookie was set with correct properties
        Assert.True(_httpContext.Response.Headers.ContainsKey("Set-Cookie"));
        var cookieHeader = _httpContext.Response.Headers["Set-Cookie"].ToString();
        Assert.Contains("clovance-refresh-token=" + expectedRefreshToken, cookieHeader);
        Assert.Contains("httponly", cookieHeader.ToLower());
        var containsSecure = cookieHeader.ToLower().Contains("secure");
        Assert.Equal(_httpContext.Request.IsHttps, containsSecure);
        Assert.Contains("samesite=strict", cookieHeader.ToLower());
    }

    [Fact]
    public async Task HandleAsync_WithLockedOutUser_ReturnsAccountLockedError()
    {
        // Arrange
        var command = new LoginCommand("test@example.com", "Password123!");

        var user = new ApplicationUser
        {
            UserName = "test@example.com",
            Email = "test@example.com"
        };

        _userManager.FindByEmailAsync(command.Email).Returns(user);
        _userManager.Users.Returns(new List<ApplicationUser> { user }.AsQueryable());
        _signInManager.CheckPasswordSignInAsync(user, command.Password, true).Returns(SignInResult.LockedOut);

        // Act
        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorCodes.Auth.AccountLocked, result.Error.Code);
        Assert.Equal(StatusCodes.Status423Locked, result.Error.StatusCode);
    }
}
