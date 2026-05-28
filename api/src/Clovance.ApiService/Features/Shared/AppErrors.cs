using Clovance.ApiService.Shared;
using FluentValidation.Results;

namespace Clovance.ApiService.Features.Shared;

public static class AppErrors
{
    public static class Common
    {
        public static Error ValidationFailed(IEnumerable<ValidationFailure> failures)
        {
            var errors = failures
                .Where(f => f is not null)
                .GroupBy(f => f.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => new
                    {
                        codes = group
                            .Select(f => NormalizeValidationErrorCode(f.ErrorCode))
                            .Distinct()
                            .ToArray()
                    });

            return new Error(
                ErrorCodes.Common.ValidationFailed,
                "Validation failed.",
                StatusCodes.Status400BadRequest,
                new Dictionary<string, object?>
                {
                    ["errors"] = errors
                });
        }

        private static string NormalizeValidationErrorCode(string? errorCode)
        {
            if (string.IsNullOrWhiteSpace(errorCode))
            {
                return ErrorCodes.Common.ValidationFailed;
            }

            return errorCode.Contains('.', StringComparison.Ordinal)
                ? errorCode
                : ErrorCodes.Common.ValidationFailed;
        }
    }

    public static class Auth
    {
        public static Error InvalidCredentials() =>
            CreateUnauthorized(ErrorCodes.Auth.InvalidCredentials, "Invalid credentials.");

        public static Error UserNotAuthenticated() =>
            CreateUnauthorized(ErrorCodes.Auth.UserNotAuthenticated, "User is not authenticated.");

        public static Error UserNotFound() =>
            CreateUnauthorized(ErrorCodes.Auth.UserNotFound, "User not found.");

        public static Error InvitationInvalidOrExpired() =>
            CreateUnauthorized(ErrorCodes.Auth.InvitationInvalidOrExpired, "Invalid or expired invitation.");

        public static Error UserAlreadyExists() =>
            CreateConflict(ErrorCodes.Auth.UserAlreadyExists, "A user with this email already exists.");

        public static Error ActiveInvitationAlreadyExists() =>
            CreateConflict(ErrorCodes.Auth.ActiveInvitationAlreadyExists, "There is already an active invitation for this email.");

        public static Error EmailAlreadyInUse() =>
            CreateConflict(ErrorCodes.Auth.EmailAlreadyInUse, "Email is already in use.");

        public static Error PasswordChangeFailed(string details) =>
            CreateConflict(ErrorCodes.Auth.PasswordChangeFailed, $"Failed to change password: {details}");

        public static Error EmailChangeFailed(string details) =>
            CreateConflict(ErrorCodes.Auth.EmailChangeFailed, $"Failed to change email: {details}");

        public static Error UsernameChangeFailed(string details) =>
            CreateConflict(ErrorCodes.Auth.UsernameChangeFailed, $"Failed to change username: {details}");

        public static Error UserCreationFailed(string details) =>
            CreateConflict(ErrorCodes.Auth.UserCreationFailed, $"Failed to create user: {details}");

        public static Error MustCompleteOnBoarding() =>
            CreateForbidden(ErrorCodes.Auth.MustCompleteOnBoarding, "User must complete onboarding.");
    }

    private static Error CreateUnauthorized(string code, string description) =>
        new(code, description, StatusCodes.Status401Unauthorized);

    private static Error CreateConflict(string code, string description) =>
        new(code, description, StatusCodes.Status409Conflict);

    private static Error CreateBadRequest(string code, string description) =>
        new(code, description, StatusCodes.Status400BadRequest);    

    private static Error CreateNotFound(string code, string description) =>
        new(code, description, StatusCodes.Status404NotFound);

    private static Error CreateForbidden(string code, string description) =>
        new(code, description, StatusCodes.Status403Forbidden);
}
