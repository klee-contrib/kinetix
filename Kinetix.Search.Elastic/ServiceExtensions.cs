using System.Text.Json;
using System.Text.Json.Serialization;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Serialization;
using Elastic.Transport;
using Kinetix.Search.Core;
using Kinetix.Search.Core.Config;
using Microsoft.Extensions.DependencyInjection;

namespace Kinetix.Search.Elastic;


public class KinetixSourceSerializer : SystemTextJsonSerializer
{
    public KinetixSourceSerializer(IElasticsearchClientSettings settings, JsonSerializerOptions options = null) =>
        Options = options ?? new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters =
            {
                    new JsonStringEnumConverter(),
                    new UnionConverter(),
                    new IdConverter(settings),
                    new RelationNameConverter(settings),
                    new RoutingConverter(settings),
                    new JoinFieldConverter(settings),
                    new LazyJsonConverter(settings),
                    new IdsConverter(settings)
            },
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
}

/// <summary>
/// Enregistre Kinetix.Search dans ASP.NET Core.
/// </summary>
public static class ServiceExtensions
{
    public static IServiceCollection AddElasticSearch(this IServiceCollection services, SearchConfig searchConfig, Action<ElasticConfigBuilder> builder)
    {
        var config = new ElasticConfigBuilder(services);
        builder(config);

        return services
            .AddSearch()
            .AddSingleton(provider =>
            {
                var server = searchConfig.GetServer(ElasticConfigBuilder.ServerName);
                var node = new Uri(server.NodeUri);
                var settings = new ElasticsearchClientSettings(
                    node,
                    (b, s) => new DefaultSourceSerializer(b, s, () =>
                    {
                        var js = new JsonSerializerSettings { DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'" };
                        if (config.JsonConverters != null)
                        {
                            foreach (var converter in config.JsonConverters)
                            {
                                js.Converters.Add(converter);
                            }
                        }
                        return js;
                    }))
                    .DisableDirectStreaming();


                foreach (var documentType in config.DocumentTypes)
                {
                    settings.DefaultMappingFor(documentType, m => m.IndexName(searchConfig.GetIndexNameForType(ElasticConfigBuilder.ServerName, documentType)));
                }

                if (!string.IsNullOrEmpty(server.Login))
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
