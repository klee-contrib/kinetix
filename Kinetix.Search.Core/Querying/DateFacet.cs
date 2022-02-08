using System.Linq.Expressions;

namespace Kinetix.Search.Core.Querying;

/// <summary>
/// Facette de date.
/// </summary>
/// <typeparam name="TDocument">Type de document.</typeparam>
public class DateFacet<TDocument> : TermFacet<TDocument>
{
    /// <summary>
    /// Constructeur.
    /// </summary>
    /// <param name="code">Code de la facette.</param>
    /// <param name="label">Libellé de la facette.</param>
    /// <param name="field">Champ sur lequel agit la facette.</param>
    public DateFacet(string code, string label, Expression<Func<TDocument, object>> field)
        : base(code, label, field)
    {
    }

    /// <inheritdoc cref="IFacetDefinition.ResolveLabel" />
    public override string ResolveLabel(string primaryKey)
    {
        return DateTime.Parse(primaryKey).ToString("dd/MM/yyyy");
    }
}
