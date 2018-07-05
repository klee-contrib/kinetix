namespace Kinetix.Search.ComponentModel
{
    /// <summary>
    /// Catégorie de champ pour le moteur de recherche.
    /// </summary>
    public enum SearchFieldCategory
    {
        /// <summary>
        /// Champ de recherche : indexé tokenisé en minuscule, non stocké.
        /// </summary>
        FullText,

        /// <summary>
        /// Champ de résultat, destiné à l'affichage : non indexé, stocké.
        /// </summary>
        Result,

        /// <summary>
        /// Champ de tri : indexé en minuscule, non stocké.
        /// </summary>
        Sort,

        /// <summary>
        /// Champ de facette : indexé tel quel, non stocké.
        /// </summary>
        Term,

        /// <summary>
        /// Champ de facette contenant une liste de valeurs : indexé tel quel, non stocké.
        /// </summary>
        Terms
    }
}
