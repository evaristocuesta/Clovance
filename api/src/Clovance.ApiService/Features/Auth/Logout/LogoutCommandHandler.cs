using Clovance.ApiService.Features.Shared;

namespace Clovance.ApiService.Features.Auth.Logout;

public sealed class LogoutCommandHandler : IHandler<LogoutCommand, Result>
{
    public async Task<Result> HandleAsync(LogoutCommand request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        return Result.Success();
    }
}
