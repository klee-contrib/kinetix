namespace Kinetix.Search.Querying;

/// <summary>
/// Tri pour les résultats de facettes.
/// </summary>
public enum FacetOrdering
{
    /// <summary>
    /// Par nombre croissant.
    /// </summary>
    CountAscending,

    /// <summary>
    /// Par nombre décroissant.
    /// </summary>
    CountDescending,

    /// <summary>
    /// Par valeur de clé croissante.
    /// </summary>
    KeyAscending,

    /// <summary>
    /// Par valeur de clé décroissante.
    /// </summary>
    KeyDescending,

    /// <summary>
    /// Pour une liste de référence, tri par ordre.
    /// </summary>
    ReferenceOrder
}
