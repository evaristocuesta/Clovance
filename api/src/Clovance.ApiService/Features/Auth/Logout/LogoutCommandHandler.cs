using Clovance.ApiService.Features.Shared;

namespace Clovance.ApiService.Features.Auth.Logout;

public sealed class LogoutCommandHandler : IHandler<LogoutCommand, Unit>
{
    public async Task<Unit> HandleAsync(LogoutCommand request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        return Unit.Value;
    }
}
