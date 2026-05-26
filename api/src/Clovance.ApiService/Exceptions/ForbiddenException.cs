namespace Clovance.ApiService.Exceptions;

public class ForbiddenException : AppException
{
    public ForbiddenException(string message = "Forbidden")
        : base(
            message,
            StatusCodes.Status403Forbidden,
            "FORBIDDEN")
    {
    }
}
