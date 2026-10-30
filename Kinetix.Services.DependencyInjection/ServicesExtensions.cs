using System.Reflection;
using AspectCore.Configuration;
using AspectCore.Extensions.DependencyInjection;
using Kinetix.Services.Annotations;
using Kinetix.Services.DependencyInjection.Interceptors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;

namespace Kinetix.Services.DependencyInjection;

public static class ServicesExtensions
{
    /// <summary>
    /// Enregistre et active les intercepteurs Kinetix pour les transactions et les traces de services sur toutes les méthodes d'interfaces annotées avec <see cref="RegisterContractAttribute"/> :
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// Une transaction sera posée (si aucune n'est déjà en cours) autour de toutes les méthodes de service, sauf si la méthode est annotée avec <see cref="NoTransactionAttribute"/>.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Une trace OpenTelemetry créée pour chaque méthode de service, sauf si <paramref name="serviceTraceFilter"/> est renseigné et retourne false pour la méthode.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// L'attribut <see cref="NoTraceAttribute"/> permet de désactiver toutes les traces émises par la méthode annotée (y compris la trace de service de l'intercepteur).
    /// </description>
    /// </item>
    /// </list>
    /// </summary>
    /// <remarks>Le conteneur d'injection de dépendances doit être configuré pour utiliser AspectCore.</remarks>
    /// <param name="services">ServiceCollection.</param>
    /// <param name="serviceTraceFilter">Filtre pour déterminer les méthodes de service sur lesquelles activer la trace.</param>
    /// <returns>ServiceCollection.</returns>
    public static IServiceCollection AddInterceptors(
        this IServiceCollection services,
        Func<MethodInfo, bool>? serviceTraceFilter = null
    )
    {
        services.AddScoped(sp => new ServiceTraceInterceptor(
            sp.GetRequiredService<ILogger<Service>>(),
            serviceTraceFilter
        ));
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
