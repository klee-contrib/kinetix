using Kinetix.Monitoring.Core;
using Microsoft.Extensions.Logging;
using Nest;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

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
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Ce que la requête retourne.</returns>
    public static async Task<T> LogQueryAsync<T>(
        this ILogger logger,
        AnalyticsManager analytics,
        string context,
        Func<CancellationToken, Task<T>> esCall,
        CancellationToken ct = default
    )
        where T : IResponse
    {
        analytics.StartProcess($"ElasticSearch.{context}", "Search");
        var response = await esCall(ct);

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(response.DebugInformation);
        }

        if (!response.ApiCall.Success)
        {
            analytics.MarkProcessInError();
            analytics.StopProcess();
            throw new ElasticException($"Error in {context}", response.DebugInformation, response.OriginalException);
        }

        var process = analytics.StopProcess();
        if (process != null && !process.Disabled)
        {
            logger.LogInformation(
                $"{context} ({response.ApiCall.HttpMethod} {response.ApiCall.Uri}) {response.ApiCall.HttpStatusCode} ({process.Duration} ms)"
            );
        }

        return response;
    }
}
