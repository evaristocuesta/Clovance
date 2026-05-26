using System.Diagnostics;
using Clovance.ApiService.Shared;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Clovance.ApiService.Exceptions;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, 
        Exception exception, 
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        _logger.LogError(
            exception,
            "Error occurred. TraceId: {TraceId}, Message: {Message}",
            traceId,
            exception.Message);

        var problemDetails = CreateProblemDetails(httpContext, exception, traceId);

        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private ProblemDetails CreateProblemDetails(
        HttpContext context,
        Exception exception,
        string traceId)
    {
        if (exception is AppException appException)
        {
            var problemDetails = new ProblemDetails
            {
                Status = appException.StatusCode,
                Title = GetTitle(appException.StatusCode),
                Detail = ResolveAppExceptionDetail(appException),
                Instance = context.Request.Path,
                Type = $"https://httpstatuses.com/{appException.StatusCode}",
                Extensions =
                {
                    ["traceId"] = traceId,
                    ["errorCode"] = appException.ErrorCode
                }
            };

            // Añadir extensiones personalizadas
            if (appException.Extensions != null)
            {
                foreach (var (key, value) in appException.Extensions)
                {
                    problemDetails.Extensions[key] = value;
                }
            }

            return problemDetails;
        }

        // Para excepciones no controladas
        var status = StatusCodes.Status500InternalServerError;

        return new ProblemDetails
        {
            Status = status,
            Title = "Internal Server Error",
            Detail = _environment.IsDevelopment()
                ? exception.Message
                : "Unexpected server error.",
            Instance = context.Request.Path,
            Type = $"https://httpstatuses.com/{status}",
            Extensions =
            {
                ["traceId"] = traceId,
                ["errorCode"] = ErrorCodes.Common.UnexpectedError
            }
        };
    }

    private static string ResolveAppExceptionDetail(AppException appException)
    {
        if (string.IsNullOrWhiteSpace(appException.Message))
        {
            return appException.ErrorCode;
        }

        return appException.Message;
    }

    private static string GetTitle(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "Bad Request",
        StatusCodes.Status401Unauthorized => "Unauthorized",
        StatusCodes.Status403Forbidden => "Forbidden",
        StatusCodes.Status404NotFound => "Not Found",
        StatusCodes.Status409Conflict => "Conflict",
        StatusCodes.Status422UnprocessableEntity => "Unprocessable Entity",
        StatusCodes.Status500InternalServerError => "Internal Server Error",
        _ => "Error"
    };
}
