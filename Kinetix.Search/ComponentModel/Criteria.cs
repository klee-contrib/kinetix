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

        /// <summary>
        /// Liste des champs à inclure dans la recherche ES.
        /// Si non renseigné (ou vide) : tous les champs seront inclus.
        /// </summary>
        public IList<string> SourceFields
        {
            get;
            set;
        }
    }
}
