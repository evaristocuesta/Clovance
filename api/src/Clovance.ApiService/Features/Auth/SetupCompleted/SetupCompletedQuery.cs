namespace Clovance.ApiService.Features.Auth.SetupCompleted;

public sealed record SetupCompletedQuery();

public sealed record SetupCompletedResult(
    bool IsSetupCompleted);
