using Clovance.ApiService.Shared;

namespace Clovance.ApiService.Exceptions;

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = "Unauthorized", string errorCode = ErrorCodes.Common.Unauthorized)
        : base(
            message,
            StatusCodes.Status401Unauthorized,
            errorCode)
    {
    }
}
