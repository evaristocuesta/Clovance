using Clovance.ApiService.Shared;

namespace Clovance.ApiService.Exceptions;

public class ForbiddenException : AppException
{
    public ForbiddenException(string message = "Forbidden", string errorCode = ErrorCodes.Common.Forbidden)
        : base(
            message,
            StatusCodes.Status403Forbidden,
            errorCode)
    {
    }
}
