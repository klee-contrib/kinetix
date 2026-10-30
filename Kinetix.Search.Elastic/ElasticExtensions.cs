using System.Diagnostics;
using Kinetix.Search.Core.Config;
using Microsoft.Extensions.Logging;
using Nest;
using OpenTelemetry;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Kinetix.Search.Elastic;

/// <summary>
/// Méthodes d'extensions pour ElasticSearch.
/// </summary>
internal static class ElasticExtensions
{
    public const string ActivitySourceName = "Kinetix.Search.Elastic";

    private static readonly ActivitySource Source = new(ActivitySourceName);

    /// <summary>
    /// Effectue la requête demandée, traite les exceptions et log le tout.
    /// </summary>
    /// <param name="logger">Logger.</param>
    /// <param name="config">Config ES.</param>
    /// <param name="method">Nom de la méthode.</param>
    /// <param name="indexType">Type de l'index concerné.</param>
    /// <param name="esCall">Appel ES.</param>
    /// <returns>Ce que la requête retourne.</returns>
    public static T LogQuery<T>(
        this ILogger logger,
        SearchConfig config,
        string method,
        Type? indexType,
        Func<T> esCall
    )
        where T : IResponse
    {
        var server = config.GetServer(ElasticConfigBuilder.ServerName);
        using var activity = Source.StartActivity(
            indexType != null ? $"{method}: {SearchConfig.GetTypeNameForIndex(indexType)}" : method,
            ActivityKind.Client,
            parentContext: default,
            new ActivityTagsCollection()
            {
                ["db.system.name"] = "es",
                ["server.address"] = $"{server.NodeUri}@{server.IndexName}",
            }
        );
        var start = Stopwatch.GetTimestamp();

        T response;

        if (Source.HasListeners())
        {
            using (SuppressInstrumentationScope.Begin())
            {
                response = esCall();
            }
        }
        else
        {
            response = esCall();
        }

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(response.DebugInformation);
        }

        if (!response.ApiCall.Success)
        {
            var ex = new ElasticException($"Error in {method}", response.DebugInformation, response.OriginalException);

            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);

            throw ex;
        }

        logger.LogInformation(
            "{Context} ({HttpMethod} {Uri}) {StatusCode} ({Duration} ms)",
            method,
            response.ApiCall.HttpMethod,
            response.ApiCall.Uri,
            response.ApiCall.HttpStatusCode,
            Stopwatch.GetElapsedTime(start).TotalMilliseconds
        );

        return response;
    }
}
