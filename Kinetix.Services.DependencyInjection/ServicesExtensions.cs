using System.Reflection;
using AspectCore.Configuration;
using AspectCore.Extensions.DependencyInjection;
using Kinetix.Services.Annotations;
using Kinetix.Services.DependencyInjection.Interceptors;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;

namespace Kinetix.Services.DependencyInjection;

public static class ServicesExtensions
{
    /// <summary>
    /// Enregistre et active les intercepteurs Kinetix pour les transactions et les traces de services.
    ///
    /// Le conteneur d'injection de dépendances doit être configuré pour utiliser AspectCore.
    /// </summary>
    /// <param name="services">ServiceCollection.</param>
    /// <returns>ServiceCollection.</returns>
    public static IServiceCollection AddInterceptors(this IServiceCollection services)
    {
        services.AddScoped<ServiceTraceInterceptor>();
        services.AddScoped<TransactionInterceptor>();
        return services.ConfigureDynamicProxy(o =>
        {
            o.Interceptors.AddServiced<ServiceTraceInterceptor>(ShouldInterceptMethod);
            o.Interceptors.AddServiced<TransactionInterceptor>(ShouldInterceptMethod);
        });
    }

    /// <summary>
    /// Enregistre les traces de service dans OpenTelemetry.
    /// </summary>
    /// <param name="builder">TracerProviderBuilder.</param>
    /// <returns>TracerProviderBuilder.</returns>
    public static TracerProviderBuilder AddService(this TracerProviderBuilder builder)
    {
        return builder.AddSource(ServiceTraceInterceptor.ActivitySourceName);
    }

    private static bool ShouldInterceptMethod(MethodInfo method)
    {
        return method.DeclaringType?.IsInterface == true
            && Attribute.IsDefined(method.DeclaringType, typeof(RegisterContractAttribute));
    }
}
