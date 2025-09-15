using System.Linq.Expressions;

namespace Kinetix.Search.Core.Querying;

/// <summary>
/// Facette de booléen.
/// </summary>
/// <typeparam name="TDocument">Type de document.</typeparam>
/// <remarks>
/// Constructeur.
/// </remarks>
/// <param name="code">Code de la facette.</param>
/// <param name="label">Libellé de la facette.</param>
/// <param name="field">Champ sur lequel agit la facette.</param>
public class BooleanFacet<TDocument>(string code, string label, Expression<Func<TDocument, object>> field)
    : TermFacet<TDocument>(code, label, field)
{
    /// <inheritdoc />
    public override bool IsMultiSelectable => false;

    /// <inheritdoc />
    public override bool CanExclude => false;

    /// <inheritdoc />
    public override FacetOrdering Ordering => FacetOrdering.KeyDescending;

    /// <inheritdoc cref="IFacetDefinition{TDocument}.ResolveLabel" />
    public override string ResolveLabel(string primaryKey)
    {
        return primaryKey == "1" || primaryKey == "true" ? "focus.search.results.yes" : "focus.search.results.no";
    }
}
