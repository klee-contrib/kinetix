using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Serialization;
using Elastic.Transport;
using Kinetix.Search.Core;
using Kinetix.Search.Core.Config;
using Microsoft.Extensions.DependencyInjection;

namespace Kinetix.Search.Elastic;

/// <summary>
/// Enregistre Kinetix.Search dans ASP.NET Core.
/// </summary>
public static class ServiceExtensions
{
    public static IServiceCollection AddElasticSearch(
        this IServiceCollection services,
        SearchConfig searchConfig,
        Action<ElasticConfigBuilder> builder
    )
    {
        var config = new ElasticConfigBuilder(services);
        builder(config);

        return services
            .AddSearch()
            .AddSingleton(provider =>
            {
                var server = searchConfig.GetServer(ElasticConfigBuilder.ServerName);
                var node = new SingleNodePool(new Uri(server.NodeUri));
                var settings = new ElasticsearchClientSettings(
                    node,
                    (_, settings) =>
                        new DefaultSourceSerializer(
                            settings,
                            js =>
                            {
                                if (config.JsonConverters != null)
                                {
                                    foreach (var converter in config.JsonConverters)
                                    {
                                        js.Converters.Add(converter);
                                    }
                                }
                            }
                        )
                ).DisableDirectStreaming();

                foreach (var documentType in config.DocumentTypes)
                {
                    settings.DefaultMappingFor(
                        documentType,
                        m =>
                            m.IndexName(searchConfig.GetIndexNameForType(ElasticConfigBuilder.ServerName, documentType))
                    );
                }

                if (!string.IsNullOrEmpty(server.Login) && !string.IsNullOrEmpty(server.Password))
                {
                    settings.Authentication(new BasicAuthentication(server.Login, server.Password));
                }

                return new ElasticsearchClient(settings);
            })
            .AddSingleton(searchConfig)
            .AddSingleton<ElasticMappingFactory>()
            .AddSingleton<FacetHandler>()
            .AddScoped<ElasticManager>()
            .AddScoped<ISearchStore, ElasticStore>();
    }
}
