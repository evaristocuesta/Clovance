using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Clovance.ApiService.Features.Shared;

public static class ResultHttpMappingExtensions
{
    public static IResult ToProblemResult(this Result result, HttpContext httpContext)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("ToProblemResult should only be used with failed results.");
        }

        var error = result.Error ?? throw new InvalidOperationException("Failed result must contain an error.");
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        var problemDetails = new ProblemDetails
        {
            Status = error.StatusCode,
            Title = GetTitle(error.StatusCode),
            Detail = error.Description,
            Instance = httpContext.Request.Path,
            Type = $"https://httpstatuses.com/{error.StatusCode}",
            Extensions =
            {
                ["traceId"] = traceId,
                ["errorCode"] = error.Code
            }
        };

        if (error.Extensions is not null)
        {
            foreach (var (key, value) in error.Extensions)
            {
                problemDetails.Extensions[key] = value;
            }
        }

        return Results.Problem(problemDetails);
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
