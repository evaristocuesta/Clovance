using Clovance.ApiService.Features.Shared;
using FluentValidation;

namespace Clovance.ApiService.Pipelines;

public sealed class ValidationDecorator<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators,
    IHandler<TRequest, TResponse> innerHandler) : IHandler<TRequest, TResponse>
{
    public async Task<TResponse> HandleAsync(TRequest command, CancellationToken cancellationToken)
    {
        var availableValidators = validators.ToArray();
        if (availableValidators.Length == 0)
        {
            return await innerHandler.HandleAsync(command, cancellationToken);
        }

        var context = new ValidationContext<TRequest>(command);

        var failures = (await Task.WhenAll(
                availableValidators.Select(v => v.ValidateAsync(context, cancellationToken))))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToArray();

        if (failures.Length == 0)
        {
            return await innerHandler.HandleAsync(command, cancellationToken);
        }

        return CreateFailureResponse(AppErrors.Common.ValidationFailed(failures));
    }

    private static TResponse CreateFailureResponse(Error error)
    {
        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(error);
        }

        if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var failureMethod = typeof(TResponse).GetMethod(
                nameof(Result<object>.Failure),
                [typeof(Error)]);

            if (failureMethod is null)
            {
                throw new InvalidOperationException($"Failure factory was not found for {typeof(TResponse).FullName}.");
            }

            return (TResponse)failureMethod.Invoke(null, [error])!;
        }

        throw new InvalidOperationException(
            $"ValidationDecorator supports only {nameof(Result)} and {nameof(Result<object>)} responses. " +
            $"Received {typeof(TResponse).FullName}.");
    }

}
