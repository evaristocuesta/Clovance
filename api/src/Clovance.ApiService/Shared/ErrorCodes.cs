namespace Clovance.ApiService.Shared;

public static class ErrorCodes
{
    public static class Common
    {
        public const string ValidationFailed = "common.validation_failed";
        public const string UnexpectedError = "common.unexpected_error";
        public const string Conflict = "common.conflict";
        public const string NotFound = "common.not_found";
        public const string Forbidden = "common.forbidden";
        public const string Unauthorized = "common.unauthorized";
        public const string BadRequest = "common.bad_request";
    }

    public static class Auth
    {
        public const string InvalidCredentials = "auth.invalid_credentials";
        public const string UserNotAuthenticated = "auth.user.not_authenticated";
        public const string UserNotFound = "auth.user.not_found";
        public const string UserAlreadyExists = "auth.user.already_exists";
        public const string UserCreationFailed = "auth.user.creation_failed";

        public const string InvitationInvalidOrExpired = "auth.invitation.invalid_or_expired";
        public const string ActiveInvitationAlreadyExists = "auth.invitation.active_already_exists";

        public const string PasswordChangeFailed = "auth.password.change_failed";
        public const string EmailAlreadyInUse = "auth.email.already_in_use";
        public const string EmailChangeFailed = "auth.email.change_failed";
        public const string UsernameChangeFailed = "auth.username.change_failed";

        public const string CurrentPasswordRequired = "auth.current_password.required";
        public const string EmailRequired = "auth.email.required";
        public const string EmailInvalid = "auth.email.invalid";
        public const string InvitationTokenRequired = "auth.invitation_token.required";

        public const string PasswordRequired = "auth.password.required";
        public const string PasswordMinLength = "auth.password.min_length";
        public const string PasswordMissingDigit = "auth.password.missing_digit";
        public const string PasswordMissingLowercase = "auth.password.missing_lowercase";
        public const string PasswordMissingUppercase = "auth.password.missing_uppercase";
        public const string PasswordMissingNonAlphanumeric = "auth.password.missing_non_alphanumeric";
    }
}
