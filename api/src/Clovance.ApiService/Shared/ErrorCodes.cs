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
        public const string UserUpdateFailed = "auth.user.update_failed";
        public const string UserDeletionFailed = "auth.user.deletion_failed";
        public const string UserAlreadyExists = "auth.user.already_exists";
        public const string UserCreationFailed = "auth.user.creation_failed";

        public const string UserIdRequired = "auth.user_id.required";
        public const string UserIdInvalid = "auth.user_id.invalid";

        public const string SetupAlreadyBeenCompleted = "auth.setup.already_been_completed";
        public const string SetupIsNotCompleted = "auth.setup.is_not_completed";

        public const string InvitationInvalidOrExpired = "auth.invitation.invalid_or_expired";
        public const string ActiveInvitationAlreadyExists = "auth.invitation.active_already_exists";

        public const string PasswordChangeFailed = "auth.password.change_failed";
        public const string EmailAlreadyInUse = "auth.email.already_in_use";
        public const string EmailChangeFailed = "auth.email.change_failed";
        public const string UsernameChangeFailed = "auth.username.change_failed";

        public const string CurrentPasswordRequired = "auth.current_password.required";
        public const string NewPasswordRequired = "auth.new_password.required";
        public const string EmailRequired = "auth.email.required";
        public const string EmailInvalid = "auth.email.invalid";
        public const string InvitationTokenRequired = "auth.invitation_token.required";

        public const string PasswordRequired = "auth.password.required";
        public const string PasswordMinLength = "auth.password.min_length";
        public const string PasswordMissingDigit = "auth.password.missing_digit";
        public const string PasswordMissingLowercase = "auth.password.missing_lowercase";
        public const string PasswordMissingUppercase = "auth.password.missing_uppercase";
        public const string PasswordMissingNonAlphanumeric = "auth.password.missing_non_alphanumeric";

        public static class FirstName
        {
            public const string Required = "auth.first_name.required";
            public const string MaxLength = "auth.first_name.max_length";
        }

        public static class LastName
        {
            public const string Required = "auth.last_name.required";
            public const string MaxLength = "auth.last_name.max_length";
        }
    }

    public static class Accounts
    {
        public const string AccountRequired = "accounts.account.required";
        public const string AccountIdRequired = "accounts.account.id_required";
        public const string AccountNotFound = "accounts.account.not_found";
        public const string AccountCreationFailed = "accounts.account.creation_failed";
        public const string AccountUpdateFailed = "accounts.account.update_failed";
        public const string AccountDeletionFailed = "accounts.account.deletion_failed";
        public const string AccountNameRequired = "accounts.account.name.required";
        public const string AccountNameMaxLength = "accounts.account.name.max_length";
        public const string AccountCurrencyInvalid = "accounts.account.currency.invalid";
    }

    public static class Transactions
    {
        public const string TransactionIdRequired = "transactions.transaction.id_required";
        public const string TransactionNotFound = "transactions.transaction.not_found";
        public const string DescriptionRequired = "transactions.transaction.description.required";
        public const string DescriptionMaxLength = "transactions.transaction.description.max_length";
        public const string AmountRequired = "transactions.transaction.amount.required";
        public const string AmountInvalid = "transactions.transaction.amount.invalid";
        public const string DateRequired = "transactions.transaction.date.required";
        public const string FilterRequired = "transactions.filter.required";
        public const string MonthInvalidRange = "transactions.month.invalid_range";
        public const string DateFromDateToMustComeTogether = "transactions.date_from_date_to.must_come_together";
        public const string PageSizeInvalidRange = "transactions.page_size.invalid_range";
        public const string CursorMustComeTogether = "transactions.cursor.must_come_together";
        public const string TypeInvalid = "transactions.type.invalid";
        public const string AmountSignTypeMismatch = "transactions.amount_sign_type.mismatch";
    }
}
