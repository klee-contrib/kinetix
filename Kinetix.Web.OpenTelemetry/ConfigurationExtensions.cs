using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Kinetix.Web.OpenTelemetry;

public static class ConfigurationExtensions
{
    /// <summary>
    /// Ajoute les processeurs pour appliquer des filtres de base sur les traces OpenTelemetry et pour annuler l'obfuscation des paramètres de query.
    /// </summary>
    /// <param name="builder">TracerProviterBuilder.</param>
    /// <param name="configureFilter">Configurateur pour le filtre.</param>
    /// <returns>TracerProviterBuilder.</returns>
    public static TracerProviderBuilder AddActivityProcessors(
        this TracerProviderBuilder builder,
        Action<ActivityFilterProcessor>? configureFilter = null
    )
    {
        var processor = new ActivityFilterProcessor();
        configureFilter?.Invoke(processor);
        return builder.AddProcessor(processor).AddProcessor<UnredactQueryParamsProcessor>();
    }

    /// <summary>
    /// Configure le service OpenTelemetry (nom, namespace, version, instanceId).
    /// </summary>
    /// <param name="services">ServiceCollection.</param>
    /// <param name="builder">Builder pour construire le service OpenTelemetry.</param>
    /// <returns></returns>
    public static IServiceCollection ConfigureOpenTelemetryService(
        this IServiceCollection services,
        Func<IHostEnvironment, OpenTelemetryService> builder
    )
    {
        void Configure(IServiceProvider provider, ResourceBuilder resourceBuilder)
        {
            var env = provider.GetRequiredService<IHostEnvironment>();
            var service = builder(env);
            resourceBuilder.AddService(
                service.Name,
                service.Namespace,
                service.Version,
                serviceInstanceId: service.InstanceId
            );
        }

        return services
            .ConfigureOpenTelemetryTracerProvider(
                (provider, builder) => builder.ConfigureResource(r => Configure(provider, r))
            )
            .ConfigureOpenTelemetryLoggerProvider(
                (provider, builder) => builder.ConfigureResource(r => Configure(provider, r))
            );
    }
}
