using System;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using Nest;

namespace Kinetix.Search.Elastic
{
    /// <summary>
    /// Méthodes d'extensions pour ElasticSearch.
    /// </summary>
    internal static class ElasticExtensions
    {
        private static readonly Stopwatch stopWatch = new Stopwatch();

        /// <summary>
        /// Effectue la requête demandée, traite les exceptions et log le tout.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="context">Contexte pour le message.</param>
        /// <param name="esCall">Appel ES.</param>
        public static T LogQuery<T>(this ILogger logger, string context, Func<T> esCall)
            where T : IResponse
        {
            stopWatch.Restart();
            var response = esCall();
            stopWatch.Stop();

            logger.LogInformation($"{context} ({response.ApiCall.HttpMethod} {response.ApiCall.Uri}) {response.ApiCall.HttpStatusCode} ({stopWatch.ElapsedMilliseconds} ms)");

            var request = response.ApiCall.RequestBodyInBytes;
            if (request != null)
            {
                var str = Encoding.UTF8.GetString(request);
                logger.LogDebug(str);
            }

            if (!response.ApiCall.Success)
            {
                throw new ElasticException($"Error in {context}", response.DebugInformation, response.OriginalException);
            }

            return response;
        }
    }
}
