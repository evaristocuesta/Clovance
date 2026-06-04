using Clovance.ApiService.Domain.RefreshTokens;
using Clovance.ApiService.Features.Auth.Logout;
using Clovance.ApiService.Infrastructure.Database;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace Clovance.UnitTests.Features.Auth;

public class LogoutCommandHandlerTests : IAsyncLifetime
{
    private readonly ClovanceDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly HttpContext _httpContext;
    private readonly LogoutCommandHandler _handler;

    public LogoutCommandHandlerTests()
    {
        _dbContext = TestDbContextFactory.CreateInMemoryDbContext();

        _httpContext = new DefaultHttpContext();
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _httpContextAccessor.HttpContext.Returns(_httpContext);

        _handler = new LogoutCommandHandler(_httpContextAccessor, _dbContext);
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
        var userId = Guid.NewGuid();

        await _dbContext.Users.AddAsync(new ApplicationUser
        {
            Id = userId.ToString(),
            Email = "user@example.com"
        }, TestContext.Current.CancellationToken);

        await _dbContext.RefreshTokens.AddAsync(
            RefreshToken.Create(userId.ToString(), "token", DateTimeOffset.UtcNow.AddDays(7)), 
            TestContext.Current.CancellationToken);

        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _httpContext.Request.Headers.Cookie = "refreshToken=token";

        var command = new LogoutCommand();

        // Act
        var result = await _handler.HandleAsync(command, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
    }
}
