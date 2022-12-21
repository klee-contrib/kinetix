using Elastic.Transport.Products.Elasticsearch;
using Kinetix.Monitoring.Core;
using Microsoft.Extensions.Logging;

namespace Kinetix.Search.Elastic;

/// <summary>
/// Méthodes d'extensions pour ElasticSearch.
/// </summary>
internal static class ElasticExtensions
{
    /// <summary>
    /// Effectue la requête demandée, traite les exceptions et log le tout.
    /// </summary>
    /// <param name="logger">Logger.</param>
    /// <param name="analytics">Analytics.</param>
    /// <param name="context">Contexte pour le message.</param>
    /// <param name="esCall">Appel ES.</param>
    public static T LogQuery<T>(this ILogger logger, AnalyticsManager analytics, string context, Func<T> esCall)
        where T : ElasticsearchResponse
    {
        analytics.StartProcess($"ElasticSearch.{context}", "Search");
        var response = esCall();

        if (!response.ApiCallDetails.HasSuccessfulStatusCode)
        {
            analytics.MarkProcessInError();
            analytics.StopProcess();
            response.TryGetOriginalException(out var exception);
            throw new ElasticException($"Error in {context}", response.DebugInformation, exception);
        }

        var process = analytics.StopProcess();
        if (!process.Disabled)
        {
            logger.LogInformation($"{context} ({response.ApiCallDetails.HttpMethod} {response.ApiCallDetails.Uri}) {response.ApiCallDetails.HttpStatusCode} ({process.Duration} ms)");
        }

        return response;
    }
}
