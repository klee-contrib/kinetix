namespace Kinetix.Search.ComponentModel
{
    /// <summary>
    /// Catégorie de champ pour le moteur de recherche.
    /// </summary>
    public enum SearchFieldCategory
    {
        /// <summary>
        /// Champ normal.
        /// </summary>
        None,

        /// <summary>
        /// Champ ID 
        /// </summary>
        Id,

        /// <summary>
        /// Champ de recherche 
        /// </summary>
        Search,

        /// <summary>
        /// Champ de filtrage de sécurité 
        /// </summary>
        Security,
    }
}