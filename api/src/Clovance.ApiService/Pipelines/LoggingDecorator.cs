using Clovance.ApiService.Features.Shared;

namespace Clovance.ApiService.Pipelines;

public sealed class LoggingDecorator<TRequest, TResponse>(
    ILogger<LoggingDecorator<TRequest, TResponse>> logger,
    IHandler<TRequest, TResponse> innerHandler) : IHandler<TRequest, TResponse>
{
    public async Task<TResponse> HandleAsync(TRequest command, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        logger.LogInformation("Handling request {RequestName}", requestName);

        try
        {
            var response = await innerHandler.HandleAsync(command, cancellationToken);

            if (response is Result result && result.IsFailure)
            {
                logger.LogWarning(
                    "Request {RequestName} failed with error code {ErrorCode}",
                    requestName,
                    result.Error?.Code);
            }
            else
            {
                logger.LogInformation("Handled request {RequestName}", requestName);
            }

            return response;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Request {RequestName} failed with unhandled exception", requestName);
            throw;
        }
    }
}
