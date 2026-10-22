using AspectCore.Configuration;
using AspectCore.Extensions.DependencyInjection;
using Kinetix.Services.DependencyInjection.Interceptors;
using Microsoft.Extensions.DependencyInjection;

namespace Kinetix.Services.DependencyInjection;

public static class ServicesExtensions
{
    /// <summary>
    /// Enregistre les intercepteurs Kinetix (pour les transactions et l'analytics).
    ///
    /// Nécessite de configurer le container d'injection de dépendances pour utiliser AspectCore.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddInterceptors(this IServiceCollection services)
    {
        services.AddScoped<AnalyticsInterceptorAttribute>();
        services.AddScoped<TransactionInterceptorAttribute>();
        return services.ConfigureDynamicProxy(o =>
        {
            o.Interceptors.AddServiced<AnalyticsInterceptorAttribute>();
            o.Interceptors.AddServiced<TransactionInterceptorAttribute>();
        });
    }
}
