using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Nest;
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
    /// <param name="context">Contexte pour le message.</param>
    /// <param name="esCall">Appel ES.</param>
    /// <returns>Ce que la requête retourne.</returns>
    public static T LogQuery<T>(this ILogger logger, string context, Func<T> esCall)
        where T : IResponse
    {
        using var activity = Source.StartActivity($"ElasticSearch.{context}", ActivityKind.Client);
        var start = Stopwatch.GetTimestamp();

        var response = esCall();

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(response.DebugInformation);
        }

        if (!response.ApiCall.Success)
        {
            var ex = new ElasticException($"Error in {context}", response.DebugInformation, response.OriginalException);

            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);

            throw ex;
        }

        logger.LogInformation(
            "{Context} ({HttpMethod} {Uri}) {StatusCode} ({Duration} ms)",
            context,
            response.ApiCall.HttpMethod,
            response.ApiCall.Uri,
            response.ApiCall.HttpStatusCode,
            Stopwatch.GetElapsedTime(start).TotalMilliseconds
        );

        return response;
    }
}
