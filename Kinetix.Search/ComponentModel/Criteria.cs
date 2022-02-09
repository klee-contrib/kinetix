using System.Collections.Generic;

namespace Kinetix.Search.ComponentModel
{
    /// <summary>
    /// Critère de recherche.
    /// </summary>
    public class Criteria
    {
        /// <summary>
        /// Critère de recherche.
        /// </summary>
        public string Query
        {
            get;
            set;
        }

        /// <summary>
        /// Liste des champs sur lesquels rechercher.
        /// </summary>
        public IList<string> SearchFields
        {
            get;
            set;
        }
    }
}
