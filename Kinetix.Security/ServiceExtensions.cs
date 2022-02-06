using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Kinetix.Security;

/// <summary>
/// Pour enregistrement.
/// </summary>
public static class ServiceExtensions
{
    /// <summary>
    /// Enregistre l'accesseur pour ICurrentUser.
    /// </summary>
    /// <param name="services">ServiceCollection.</param>
    /// <returns>ServiceCollection.</returns>
    public static IServiceCollection AddSecurity(this IServiceCollection services)
    {
        return services
            .AddScoped(provider =>
            {
                var httpContext = provider.GetService<IHttpContextAccessor>()?.HttpContext;
                return httpContext == null
                    ? throw new InvalidOperationException("Impossible d'utiliser ICurrentUser dans ce contexte.")
                    : new WebUser(httpContext);
            });
    }

    /// <summary>
    /// Enregistre l'accesseur pour ICurrentUser.
    /// </summary>
    /// <typeparam name="T">Implémentation de ICurrentUser à utiliser hors contexte HTTP ou en authent anonyme.</typeparam>
    /// <param name="services">ServiceCollection.</param>
    /// <returns>ServiceCollection.</returns>
    public static IServiceCollection AddSecurity<T>(this IServiceCollection services)
        where T : class, ICurrentUser
    {
        return services
            .AddScoped<T>()
            .AddScoped(provider =>
            {
                var httpContext = provider.GetService<IHttpContextAccessor>()?.HttpContext;
                return httpContext != null && httpContext.User.Identity.IsAuthenticated
                    ? new WebUser(httpContext)
                    : (ICurrentUser)provider.GetRequiredService<T>();
            });
    }
}
