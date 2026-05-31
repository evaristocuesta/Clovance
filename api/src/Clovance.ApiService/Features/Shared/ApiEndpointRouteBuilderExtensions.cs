using System.Reflection;

namespace Clovance.ApiService.Features.Shared;

public static class ApiEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder RegisterApiEndpointsFromAssembly(
        this IEndpointRouteBuilder app,
        Assembly assembly,
        string? prefix = "api")
    {
        var endpointTypes = assembly
            .DefinedTypes
            .Where(type => type is { IsAbstract: false, IsInterface: false })
            .Where(type => typeof(IApiEndPoint).IsAssignableFrom(type.AsType()))
            .Select(type => type.AsType())
            .ToArray();

        // Group endpoints by feature (extracted from namespace)
        var featureGroups = endpointTypes
            .Select(type => new
            {
                Type = type,
                Feature = GetFeatureFromNamespace(type)
            })
            .Where(x => x.Feature != null)
            .GroupBy(x => x.Feature);

        foreach (var featureGroup in featureGroups)
        {
            var groupPath = string.IsNullOrWhiteSpace(prefix)
                ? $"/{featureGroup.Key!.ToLowerInvariant()}"
                : $"/{prefix}/{featureGroup.Key!.ToLowerInvariant()}";

            var routeGroup = app.MapGroup(groupPath)
                .WithTags(featureGroup.Key);

            foreach (var item in featureGroup)
            {
                if (Activator.CreateInstance(item.Type) is IApiEndPoint endpoint)
                {
                    endpoint.MapApiEndpoints(routeGroup);
                }
            }
        }

        return app;
    }

    private static string? GetFeatureFromNamespace(Type type)
    {
        // Extract feature name from namespace
        // e.g., "Clovance.ApiService.Features.Auth.Login" -> "Auth"
        var namespaceParts = type.Namespace?.Split('.');

        if (namespaceParts is null) return null;

        var featuresIndex = Array.FindIndex(namespaceParts, p => p == "Features");

        return featuresIndex >= 0 && featuresIndex < namespaceParts.Length - 1
            ? namespaceParts[featuresIndex + 1]
            : null;
    }
}
