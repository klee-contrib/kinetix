using System;
using Kinetix.Search.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kinetix.Search.Elastic
{
    /// <summary>
    /// Enregistre Kinetix.Search dans ASP.NET Core.
    /// </summary>
    public static class ServiceExtensions
    {
        public static IServiceCollection AddElasticSearch(this IServiceCollection services, Action<ElasticConfigBuilder> builder)
        {
            var config = new ElasticConfigBuilder(services);
            builder(config);

            return services
                .AddSearch(config.DefaultDataSourceName)
                .AddSingleton<ElasticMappingFactory>()
                .AddSingleton(provider => new ElasticManager(provider.GetService<ILogger<ElasticManager>>(), provider.GetService<IOptions<SearchConfig>>(), config.DocumentTypes, config.JsonConverters))
                .AddTransient(typeof(ISearchStore<>), typeof(ElasticStore<>));
        }

        public static ElasticStore<TDocument> GetElasticStore<TDocument>(this SearchManager manager)
            where TDocument : class, new()
        {
            return (ElasticStore<TDocument>)manager.GetStore<TDocument>();
        }
    }
}
