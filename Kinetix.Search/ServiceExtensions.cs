using System;
using Kinetix.Search.Config;
using Kinetix.Search.Contract;
using Kinetix.Search.Elastic;
using Kinetix.Search.MetaModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kinetix.Search
{
    /// <summary>
    /// Enregistre Kinetix.Search dans ASP.NET Core.
    /// </summary>
    public static class ServiceExtensions
    {
        public static IServiceCollection AddSearch(this IServiceCollection services, Action<SearchConfigBuilder> builder)
        {
            var config = new SearchConfigBuilder(services);
            builder(config);

            return services
                .AddSingleton<DocumentDescriptor>()
                .AddSingleton<ElasticMappingFactory>()
                .AddSingleton(provider => new ElasticManager(provider.GetService<ILogger<ElasticManager>>(), provider.GetService<IOptions<SearchConfig>>(), config.DocumentTypes, config.JsonConverters))
                .AddTransient(typeof(ISearchStore<>), typeof(ElasticStore<>))
                .AddSingleton(provider => new SearchManager(config.DefaultDataSourceName, provider));
        }
    }
}
