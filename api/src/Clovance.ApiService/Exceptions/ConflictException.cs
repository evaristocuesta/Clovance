namespace Clovance.ApiService.Exceptions;

public class ConflictException : AppException
{
    public ConflictException(string message)
        : base(
            message,
            StatusCodes.Status409Conflict,
            "CONFLICT")
    {
    }
}
