using System.Collections.Generic;

namespace Kinetix.Search.Models
{
    /// <summary>
    /// Sortie d'une recherche avancée.
    /// </summary>
    public class QueryOutput
    {
        /// <summary>
        /// Groupe de liste de résultats.
        /// </summary>
        public IList<GroupResult> Groups
        {
            get;
            set;
        }

        /// <summary>
        /// Facettes sélectionnées.
        /// </summary>
        public IList<FacetOutput> Facets
        {
            get;
            set;
        }

        /// <summary>
        /// Nombre total d'éléments.
        /// </summary>
        public long? TotalCount
        {
            get;
            set;
        }
    }
}
