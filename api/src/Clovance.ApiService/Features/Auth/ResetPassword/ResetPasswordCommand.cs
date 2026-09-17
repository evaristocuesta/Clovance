namespace Clovance.ApiService.Features.Auth.ResetPassword;

public sealed record ResetPasswordCommand(string Email, string Token, string NewPassword);
