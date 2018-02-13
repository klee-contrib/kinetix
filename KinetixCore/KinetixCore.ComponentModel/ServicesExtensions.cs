using Microsoft.Extensions.DependencyInjection;

namespace Kinetix.ComponentModel
{
    /// <summary>
    /// Méthodes d'extensions.
    /// </summary>
    public static class ServiceExtensions
    {
        public static void AddComponentModel<DomainMetadata>(this IServiceCollection services)
        {
            services.AddSingleton<IDomainManager, DomainManager<DomainMetadata>>();
            services.AddSingleton<BeanDescriptor>();
        }
    }
}
