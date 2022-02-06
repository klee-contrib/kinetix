using Elasticsearch.Net;
using Kinetix.Search.Config;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using Nest.JsonNetSerializer;
using Newtonsoft.Json;

namespace Kinetix.Search.Elastic;

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
                var settings = new ConnectionSettings(
                    new SingleNodeConnectionPool(node),
                    (b, s) => new JsonNetSerializer(b, s, () =>
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
                    settings.BasicAuthentication(server.Login, server.Password);
                }

                return new ElasticClient(settings);
            })
            .AddSingleton(searchConfig)
            .AddSingleton<ElasticMappingFactory>()
            .AddSingleton<FacetHandler>()
            .AddScoped<ElasticManager>()
            .AddScoped<ISearchStore, ElasticStore>();
    }
}
