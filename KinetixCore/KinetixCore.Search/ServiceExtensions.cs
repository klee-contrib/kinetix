using Kinetix.Search.Contract;
using Kinetix.Search.Elastic;
using Kinetix.Search.MetaModel;
using Microsoft.Extensions.DependencyInjection;

namespace Kinetix.Search
{
    /// <summary>
    /// Enregistre Kinetix.Search dans ASP.NET Core.
    /// </summary>
    public static class ServiceExtensions
    {
        public static IServiceCollection AddSearch(this IServiceCollection services, string defaultDataSourceName)
        {
            // TODO Add default implementation for monitoring when used without the full Monitoring solution.
            services.AddSingleton<DocumentDescriptor>();
            services.AddSingleton<ElasticManager>();
            services.AddTransient(typeof(ISearchStore<>), typeof(ElasticStore<>));
            services.AddSingleton(provider => new SearchManager(defaultDataSourceName, provider));
            return services;
        }
    }
}
