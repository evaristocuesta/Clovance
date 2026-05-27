using System.Reflection;
using Clovance.ApiService.Pipelines;
using FluentValidation;

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
            services.AddScoped(registration.Implementation);

            foreach (var serviceType in registration.Interfaces)
            {
                services.AddScoped(serviceType, serviceProvider =>
                {
                    var requestType = serviceType.GenericTypeArguments[0];
                    var responseType = serviceType.GenericTypeArguments[1];

                    var baseHandler = serviceProvider.GetRequiredService(registration.Implementation);

                    var validationDecoratorType = typeof(ValidationDecorator<,>).MakeGenericType(requestType, responseType);
                    var validationDecorator = ActivatorUtilities.CreateInstance(serviceProvider, validationDecoratorType, baseHandler);

                    var loggingDecoratorType = typeof(LoggingDecorator<,>).MakeGenericType(requestType, responseType);
                    var loggingDecorator = ActivatorUtilities.CreateInstance(serviceProvider, loggingDecoratorType, validationDecorator);

                    return loggingDecorator;
                });
            }
        }

        return services;
    }
}
