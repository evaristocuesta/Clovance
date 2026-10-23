using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace Clovance.UnitTests.Features;

/// <summary>
/// Wraps a LINQ-to-Objects sequence as an <see cref="IAsyncQueryProvider"/> so Entity
/// Framework's asynchronous operators (for example AnyAsync) can run against mocked
/// queryables in unit tests.
/// </summary>
public sealed class AsyncQueryable<T> : EnumerableQuery<T>, IAsyncQueryProvider
{
    public AsyncQueryable(IEnumerable<T> source)
        : base(source)
    {
    }

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        var resultElementType = typeof(TResult).GetGenericArguments()[0];

        var syncResult = typeof(IQueryProvider)
            .GetMethods()
            .Single(method => method.Name == nameof(IQueryProvider.Execute) && method.IsGenericMethodDefinition)
            .MakeGenericMethod(resultElementType)
            .Invoke(this, new object[] { expression });

        return (TResult)typeof(Task)
            .GetMethods()
            .Single(method => method.Name == nameof(Task.FromResult) && method.IsGenericMethodDefinition)
            .MakeGenericMethod(resultElementType)
            .Invoke(null, new[] { syncResult })!;
    }
}

public static class AsyncQueryable
{
    public static AsyncQueryable<T> AsAsyncQueryable<T>(this IEnumerable<T> source) => new(source);
}
