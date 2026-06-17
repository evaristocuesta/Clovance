namespace Clovance.ApiService.Features.Auth.RegisterAdmin;

public sealed record SetupCommand(string Email, string Password, string FirstName = "", string LastName = "");
