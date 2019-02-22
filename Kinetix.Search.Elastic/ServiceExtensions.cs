using System;
using Elasticsearch.Net;
using Kinetix.Search.Config;
using Kinetix.Search.Elastic.Faceting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using Nest.JsonNetSerializer;
using Newtonsoft.Json;

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
                .AddSearch(ElasticConfigBuilder.ServerName)
                .AddSingleton(provider =>
                {
                    var searchConfig = provider.GetService<IOptions<SearchConfig>>().Value;
                    var server = searchConfig.GetServer(ElasticConfigBuilder.ServerName);
                    var node = new Uri(server.NodeUri);
                    var settings = new ConnectionSettings(
                        new SingleNodeConnectionPool(node),
                        (b, s) => new JsonNetSerializer(b, s, () =>
                        {
                            var js = new JsonSerializerSettings();
                            if (config.JsonConverters != null)
                            {
                                foreach (var converter in config.JsonConverters)
                                {
                                    js.Converters.Add(converter);
                                }
                            }
                            return js;
                        }))
                        .DefaultIndex(server.IndexName)
                        .DisableDirectStreaming();

                    foreach (var documentType in config.DocumentTypes)
                    {
                        settings.DefaultMappingFor(documentType, m => m.IndexName(searchConfig.GetIndexNameForType(ElasticConfigBuilder.ServerName, documentType)));
                    }

                    return new ElasticClient(settings);
                })
                .AddSingleton<ElasticMappingFactory>()
                .AddSingleton<StandardFacetHandler>()
                .AddSingleton<PortfolioFacetHandler>()
                .AddSingleton(provider => new ElasticManager(
                    provider.GetService<ILogger<ElasticManager>>(),
                    provider.GetService<IOptions<SearchConfig>>(),
                    provider.GetService<ElasticClient>(),
                    config.DocumentTypes))
                .AddSingleton<ISearchStore, ElasticStore>();
        }
    }
}
