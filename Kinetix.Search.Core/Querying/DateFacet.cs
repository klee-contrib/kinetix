using System.Linq.Expressions;

namespace Kinetix.Search.Core.Querying;

/// <summary>
/// Facette de date.
/// </summary>
/// <typeparam name="TDocument">Type de document.</typeparam>
/// <remarks>
/// Constructeur.
/// </remarks>
/// <param name="code">Code de la facette.</param>
/// <param name="label">Libellé de la facette.</param>
/// <param name="field">Champ sur lequel agit la facette.</param>
public class DateFacet<TDocument>(string code, string label, Expression<Func<TDocument, object>> field)
    : TermFacet<TDocument>(code, label, field)
{
    /// <inheritdoc cref="IFacetDefinition{TDocument}.ResolveLabelAsync" />
    public override Task<string> ResolveLabelAsync(string primaryKey, CancellationToken ct = default)
    {
        return Task.FromResult(DateTime.Parse(primaryKey).ToString("dd/MM/yyyy"));
    }
}
