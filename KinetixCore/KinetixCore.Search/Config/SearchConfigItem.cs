namespace Kinetix.Search.Config
{
    /// <summary>
    /// Configuration d'une datasource de moteur de recherche.
    /// </summary>
    public class SearchConfigItem
    {
        /// <summary>
        /// URL du noeud.
        /// </summary>
        public string NodeUri
        {
            get;
            set;
        }

        /// <summary>
        /// Nom de l'index.
        /// </summary>
        public string IndexName
        {
            get;
            set;
        }
    }
}
