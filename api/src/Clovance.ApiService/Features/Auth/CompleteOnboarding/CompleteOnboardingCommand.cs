namespace Clovance.ApiService.Features.Auth.CompleteOnboarding;

public sealed record CompleteOnboardingCommand(
    string CurrentPassword,
    string NewPassword,
    string NewEmail);
