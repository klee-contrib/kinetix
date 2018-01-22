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
        public static void AddSearch(this IServiceCollection services, string defaultDataSourceName)
        {
            services.AddSingleton<DocumentDescriptor>();
            services.AddSingleton<ElasticManager>();
            services.AddTransient(typeof(ISearchStore<>), typeof(ElasticStore<>));
            services.AddSingleton(provider => new SearchManager(defaultDataSourceName, provider));
        }
    }
}
