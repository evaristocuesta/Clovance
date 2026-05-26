using Clovance.ApiService.Shared;

namespace Clovance.ApiService.Exceptions;

public class BadRequestException : AppException
{
    public BadRequestException(string message, string errorCode = ErrorCodes.Common.BadRequest)
        : base(
            message,
            StatusCodes.Status400BadRequest,
            errorCode)
    {
    }
}
