namespace Clovance.ApiService.Exceptions;

public class BadRequestException : AppException
{
    public BadRequestException(string message)
        : base(
            message,
            StatusCodes.Status400BadRequest,
            "BAD_REQUEST")
    {
    }
}
