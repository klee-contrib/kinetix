using Kinetix.Search.ComponentModel;

namespace Kinetix.Search.Model
{
    /// <summary>
    /// Entrée complète d'une recherche avancée.
    /// </summary>
    public class AdvancedQueryInput<TDocument, TCriteria>
         where TCriteria : Criteria, new()
    {
        /// <summary>
        /// Entrée de l'API.
        /// </summary>
        public QueryInput<TCriteria> ApiInput
        {
            get;
            set;
        }

        /// <summary>
        /// Définition de la recherhe à facette.
        /// </summary>
        public FacetQueryDefinition FacetQueryDefinition
        {
            get;
            set;
        }

        /// <summary>
        /// Filtrage de sécurité.
        /// </summary>
        public string Security
        {
            get;
            set;
        }

        /// <summary>
        /// Portefeuille de l'utilisateur.
        /// </summary>
        public string Portfolio
        {
            get;
            set;
        }

        /// <summary>
        /// Critères supplémentaires.
        /// </summary>
        public TDocument AdditionalCriteria
        {
            get;
            set;
        }

        /// <summary>
        /// Nombre d'éléments à récupérer dans un groupe.
        /// </summary>
        public int GroupSize { get; set; } = 10;
    }
}
