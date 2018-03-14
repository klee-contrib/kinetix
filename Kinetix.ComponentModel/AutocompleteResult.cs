using System.Collections.Generic;

namespace Kinetix.ComponentModel
{
    /// <summary>
    /// Item d'un résultat d'autocomplete.
    /// </summary>
    public class AutocompleteItem
    {
        /// <summary>
        /// Clé.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Libellé.
        /// </summary>
        public string Label { get; set; }
    }

    /// <summary>
    /// Résultat d'une requête d'autocomplete.
    /// </summary>
    public class AutocompleteResult
    {
        /// <summary>
        /// Les données.
        /// </summary>
        public ICollection<AutocompleteItem> Data { get; set; }

        /// <summary>
        /// Le nombre total de résultats.
        /// </summary>
        public int TotalCount { get; set; }
    }
}
