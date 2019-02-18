using System;
using System.Collections.Generic;
using Kinetix.Search.ComponentModel;
using Kinetix.Search.Model;

namespace Kinetix.Search
{
    /// <summary>
    /// Extensions pour ISearchStore.
    /// </summary>
    public static class SearchStoreExtensions
    {
        /// <summary>
        /// Effectue une requête sur le champ texte.
        /// </summary>
        /// <param name="store">Store de recherche.</param>
        /// <param name="queryInput">Query input.</param>
        /// <returns>Résultat.</returns>
        public static (IEnumerable<TDocument> data, int totalCount) Query<TDocument>(this ISearchStore store, BasicQueryInput<TDocument> queryInput)
            where TDocument : class
        {
            return store.Query(queryInput, x => x);
        }

        /// <summary>
        /// Effectue une requête sur le champ texte.
        /// </summary>
        /// <param name="store">Store de recherche.</param>
        /// <param name="queryInput">Query input.</param>
        /// <param name="documentMapper">Mapper de document.</param>
        /// <returns>Résultat.</returns>
        public static (IEnumerable<TOutput> data, int totalCount) Query<TDocument, TOutput>(this ISearchStore store, BasicQueryInput<TDocument> queryInput, Func<TDocument, TOutput> documentMapper)
            where TDocument : class
        {
            if (string.IsNullOrEmpty(queryInput.Query))
            {
                return (new List<TOutput>(), 0);
            }

            var input = new AdvancedQueryInput<TDocument, Criteria>
            {
                ApiInput = new QueryInput
                {
                    Criteria = new Criteria
                    {
                        Query = queryInput.Query
                    },
                    Skip = 0,
                    Top = queryInput.Top ?? 10
                },
                Security = queryInput.Security,
                AdditionalCriteria = queryInput.Criteria
            };

            var output = store.AdvancedQuery(input, documentMapper);
            return (output.List, output.TotalCount.Value);
        }
    }
}
