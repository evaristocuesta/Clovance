using System.Reflection;

namespace Clovance.ApiService.Features.Shared;

public static class HandlerServiceCollectionExtensions
{
    public static IServiceCollection AddHandlersFromAssembly(this IServiceCollection services, Assembly assembly)
    {
        var handlerInterfaceType = typeof(IHandler<,>);

        var handlerRegistrations = assembly
            .DefinedTypes
            .Where(type => type is { IsAbstract: false, IsInterface: false })
            .Select(type => new
            {
                Implementation = type.AsType(),
                Interfaces = type.ImplementedInterfaces
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterfaceType)
                    .ToArray()
            })
            .Where(x => x.Interfaces.Length > 0)
            .ToArray();

        foreach (var registration in handlerRegistrations)
        {
            foreach (var serviceType in registration.Interfaces)
            {
                services.AddScoped(serviceType, registration.Implementation);
            }
        }

        return services;
    }
}
