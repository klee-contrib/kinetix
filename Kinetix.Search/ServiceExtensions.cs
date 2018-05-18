using System;
using Kinetix.Search.Config;
using Kinetix.Search.Contract;
using Kinetix.Search.Elastic;
using Kinetix.Search.MetaModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Kinetix.Search
{
    /// <summary>
    /// Enregistre Kinetix.Search dans ASP.NET Core.
    /// </summary>
    public static class ServiceExtensions
    {
        public static IServiceCollection AddSearch<TMappingFactory>(this IServiceCollection services, string defaultDataSourceName, Type[] documentTypes, JsonConverter[] jsonConverters = null)
            where TMappingFactory : ElasticMappingFactory
        {
            return services.AddSearchCore(defaultDataSourceName, documentTypes, jsonConverters)
                .AddSingleton<ElasticMappingFactory, TMappingFactory>();
        }

        public static IServiceCollection AddSearch(this IServiceCollection services, string defaultDataSourceName, Type[] documentTypes, JsonConverter[] jsonConverters = null)
        {
            return services.AddSearchCore(defaultDataSourceName, documentTypes, jsonConverters)
                .AddSingleton<ElasticMappingFactory>();
        }

        private static IServiceCollection AddSearchCore(this IServiceCollection services, string defaultDataSourceName, Type[] documentTypes, JsonConverter[] jsonConverters = null)
        {
            return services.AddSingleton<DocumentDescriptor>()
                 .AddSingleton(provider => new ElasticManager(provider.GetService<ILogger<ElasticManager>>(), provider.GetService<IOptions<SearchConfig>>(), documentTypes, jsonConverters))
                 .AddTransient(typeof(ISearchStore<>), typeof(ElasticStore<>))
                 .AddSingleton(provider => new SearchManager(defaultDataSourceName, provider));
        }
    }
}
