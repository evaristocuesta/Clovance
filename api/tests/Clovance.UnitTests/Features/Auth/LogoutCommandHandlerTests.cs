using Clovance.ApiService.Features.Auth.Logout;

namespace Clovance.UnitTests.Features.Auth;

public class LogoutCommandHandlerTests
{
    private readonly LogoutCommandHandler _handler;

    public LogoutCommandHandlerTests()
    {
        _handler = new LogoutCommandHandler();
    }

    [Fact]
    public async Task HandleAsync_ReturnsSuccess()
    {
        var command = new LogoutCommand();

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }
}
