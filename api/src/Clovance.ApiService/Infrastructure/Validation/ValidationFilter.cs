using FluentValidation;
using Clovance.ApiService.Shared;

namespace Clovance.ApiService.Infrastructure.Validation;

public class ValidationFilter<T> : IEndpointFilter where T : class
{
    private readonly IValidator<T>? _validator;

    public ValidationFilter(IServiceProvider serviceProvider)
    {
        _validator = serviceProvider.GetService<IValidator<T>>();
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (_validator is null)
        {
            return await next(context);
        }

        var argument = context.Arguments.OfType<T>().FirstOrDefault();
        if (argument is null)
        {
            return await next(context);
        }

        var validationResult = await _validator.ValidateAsync(argument);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => new
                    {
                        codes = group
                            .Select(e => NormalizeValidationErrorCode(e.ErrorCode))
                            .Distinct()
                            .ToArray()
                    });

            return Results.BadRequest(new
            {
                errorCode = ErrorCodes.Common.ValidationFailed,
                errors
            });
        }

        return await next(context);
    }

    private static string NormalizeValidationErrorCode(string? errorCode)
    {
        if (string.IsNullOrWhiteSpace(errorCode))
        {
            return ErrorCodes.Common.ValidationFailed;
        }

        // FluentValidation built-in codes are typically names like "NotEmptyValidator".
        // Application codes follow a dotted format (e.g. "auth.email.invalid").
        return errorCode.Contains('.', StringComparison.Ordinal)
            ? errorCode
            : ErrorCodes.Common.ValidationFailed;
    }
}
