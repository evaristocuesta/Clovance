using Clovance.ApiService.Shared;

namespace Clovance.ApiService.Exceptions;

public class ConflictException : AppException
{
    public ConflictException(string message, string errorCode = ErrorCodes.Common.Conflict)
        : base(
            message,
            StatusCodes.Status409Conflict,
            errorCode)
    {
    }
}
