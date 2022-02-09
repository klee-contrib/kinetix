using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Nest;

namespace Kinetix.Search.Elastic
{
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
        public static T LogQuery<T>(this ILogger logger, SearchAnalytics analytics, string context, Func<T> esCall)
            where T : IResponse
        {
            analytics.StartQuery($"ElasticSearch.{context}");
            var response = esCall();

            if (!response.ApiCall.Success)
            {
                analytics.StopQueryInError();
                throw new ElasticException($"Error in {context}", response.DebugInformation, response.OriginalException);
            }

            var duration = analytics.StopQuery();
            logger.LogInformation($"{context} ({response.ApiCall.HttpMethod} {response.ApiCall.Uri}) {response.ApiCall.HttpStatusCode} ({duration} ms)");

            return response;
        }

        /// <summary>
        /// Enumère des objets en clusters.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="source">Source.</param>
        /// <param name="size">Taille du cluster.</param>
        /// <returns>Enumération de clusters.</returns>
        public static IEnumerable<IList<T>> SelectCluster<T>(this IEnumerable<T> source, int size)
        {
            var list = new List<T>();
            foreach (var item in source)
            {
                list.Add(item);
                if (list.Count == size)
                {
                    yield return list;
                    list = new List<T>();
                }
            }

            if (list.Count > 0)
            {
                yield return list;
            }
        }
    }
}
