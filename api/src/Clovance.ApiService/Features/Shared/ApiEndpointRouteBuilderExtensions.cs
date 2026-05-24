using System.Reflection;

namespace Clovance.ApiService.Features.Shared;

public static class ApiEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder RegisterApiEndpointsFromAssembly(this IEndpointRouteBuilder app, Assembly assembly)
    {
        var endpointTypes = assembly
            .DefinedTypes
            .Where(type => type is { IsAbstract: false, IsInterface: false })
            .Where(type => typeof(IApiEndPoint).IsAssignableFrom(type.AsType()))
            .Select(type => type.AsType())
            .ToArray();

        foreach (var endpointType in endpointTypes)
        {
            if (Activator.CreateInstance(endpointType) is IApiEndPoint endpoint)
            {
                endpoint.MapApiEndpoints(app);
            }
        }

        return app;
    }
}
