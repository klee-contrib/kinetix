using System.Linq.Expressions;
using Kinetix.Modeling;
using Kinetix.Search.Models;
using Kinetix.Services;

namespace Kinetix.Search.Core.Querying;

/// <summary>
/// Facette de référence.
/// </summary>
/// <typeparam name="TDocument">Type de document.</typeparam>
/// <remarks>
/// Constructeur.
/// </remarks>
/// <param name="code">Code de la facette.</param>
/// <param name="label">Libellé de la facette.</param>
/// <param name="field">Champ sur lequel agit la facette.</param>
public abstract class ReferenceFacet<TDocument>(string code, string label, Expression<Func<TDocument, object>> field)
    : TermFacet<TDocument>(code, label, field)
{
    /// <summary>
    /// Affiche l'intégralité des valeurs de la liste de référence dans les résultats de facettes, même si les buckets sont vides.
    /// </summary>
    public bool ShowEmptyReferenceValues { get; set; } = false;

    /// <summary>
    /// Récupère la liste de référence associée à la facette.
    /// </summary>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Liste de référence.</returns>
    public abstract Task<IList<FacetItem>> GetReferenceListAsync(CancellationToken ct = default);
}

/// <summary>
/// Facette de référence.
/// </summary>
/// <typeparam name="TDocument">Type de document.</typeparam>
/// <typeparam name="T">Type de la référence.</typeparam>
/// <remarks>
/// Constructeur.
/// </remarks>
/// <param name="referenceManager">ReferenceManager.</param>
/// <param name="code">Code de la facette.</param>
/// <param name="label">Libellé de la facette.</param>
/// <param name="field">Champ sur lequel agit la facette.</param>
public class ReferenceFacet<TDocument, T>(
    IReferenceManager referenceManager,
    string code,
    string label,
    Expression<Func<TDocument, object>> field
) : ReferenceFacet<TDocument>(code, label, field)
    where T : notnull
{
    /// <inheritdoc />
    public override async Task<IList<FacetItem>> GetReferenceListAsync(CancellationToken ct = default)
    {
        var def = BeanDescriptor.GetDefinition(typeof(T));
        return (await referenceManager.GetReferenceListAsync<T>(ct: ct))
            .Select(item => new FacetItem
            {
                Code = def.PrimaryKey.GetValue(item)!.ToString()!,
                Label = (string)(def.DefaultProperty?.GetValue(item) ?? string.Empty),
                Count = 0,
            })
            .ToList();
    }

    /// <inheritdoc cref="IFacetDefinition{TDocument}.ResolveLabelAsync" />
    public override async Task<string> ResolveLabelAsync(string primaryKey, CancellationToken ct = default)
    {
        return (await referenceManager.GetReferenceValueAsync<T>(primaryKey, ct)) ?? string.Empty;
    }
}
