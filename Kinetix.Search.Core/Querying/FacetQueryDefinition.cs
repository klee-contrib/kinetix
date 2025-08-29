namespace Kinetix.Search.Core.Querying;

/// <summary>
/// Définition d'une recherche à facettes.
/// </summary>
/// <typeparam name="TDocument">Type de document.</typeparam>
/// <remarks>
/// Créé une nouvelle instance de FacetQueryDefinition.
/// </remarks>
/// <param name="facets">Facettes.</param>
public class FacetQueryDefinition<TDocument>(params IFacetDefinition<TDocument>[] facets)
{
    /// <summary>
    /// Libellé de la valeur de facette nulle.
    /// </summary>
    public string? FacetNullValueLabel
    {
        get;
        set;
    }

    /// <summary>
    /// Liste des facettes.
    /// </summary>
    public ICollection<IFacetDefinition<TDocument>> Facets
    {
        get;
        private set;
    }

= facets.ToList();
}
