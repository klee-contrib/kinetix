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
        /// Effectue une requête sur le champ text.
        /// </summary>
        /// <param name="store">Store de recherche.</param>
        /// <param name="text">Texte à chercher.</param>
        /// <param name="filterList">Filtres.</param>
        /// <param name="security">Filtrage de périmètre de sécurité.</param>
        /// <param name="top">Nombre de résultats désirés.</param>
        /// <returns>Documents trouvés et le nombre total de résultats .</returns>
        public static (IEnumerable<TDocument> data, int totalCount) Query<TDocument>(this ISearchStore<TDocument> store, string text, IDictionary<string, string> filterList = null, string security = null, int top = 10)
            where TDocument : class
        {
            if (string.IsNullOrEmpty(text))
            {
                return (new List<TDocument>(), 0);
            }

            var input = new AdvancedQueryInput
            {
                ApiInput = new QueryInput
                {
                    Criteria = new Criteria
                    {
                        Query = text
                    },
                    Skip = 0,
                    Top = top
                },
                Security = security,
                FilterList = filterList
            };
            var output = store.AdvancedQuery(input);
            return (output.List, output.TotalCount.Value);
        }
    }
}
