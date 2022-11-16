using System.Text.Json.Serialization;
using Kinetix.Web.Exceptions;
using Kinetix.Web.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Kinetix.Web;

/// <summary>
/// Extensions pour configurer Kinetix.Web.
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Enregistre tous les filtres MVC de Kinetix dans MVC.
    /// </summary>
    /// <param name="builder">Builder.</param>
    /// <returns>Builder.</returns>
    public static MvcOptions AddWeb(this MvcOptions builder)
    {
        builder.Filters.AddService<CultureFilter>();
        builder.Filters.AddService<ExceptionFilter>();
        builder.Filters.AddService<TransactionFilter>();
        builder.Filters.AddService<ReferenceCheckerFilter>();
        builder.Filters.AddService<UtcDateFilter>();

        return builder;
    }

    /// <summary>
    /// Enregistre tous les filtres MVC de Kinetix dans la DI.
    /// </summary>
    /// <param name="services">Services.</param>
    /// <returns>Services.</returns>
    public static IServiceCollection AddWeb(this IServiceCollection services)
    {
        return services
            .AddScoped<CultureFilter>()
            .AddScoped<ExceptionFilter>()
            .AddSingleton<IExceptionHandler, BusinessExceptionHandler>()
            .AddSingleton<IExceptionHandler, MissingEntityExceptionHandler>()
            .AddScoped<TransactionFilter>()
            .AddScoped<ReferenceCheckerFilter>()
            .AddScoped<UtcDateFilter>();
    }

    /// <summary>
    /// Configuration par défaut pour la sérialisation JSON.
    /// </summary>
    /// <param name="options">JSONOptions.</param>
    public static void ConfigureSerializer(this JsonOptions options)
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
        options.JsonSerializerOptions.Converters.Add(new TimeSpanConverter());
    }
}
