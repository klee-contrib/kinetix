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
    /// <returns>Liste de référence.</returns>
    public abstract IList<FacetItem> GetReferenceList();
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
    public override IList<FacetItem> GetReferenceList()
    {
        var def = BeanDescriptor.GetDefinition(typeof(T));
        return referenceManager
            .GetReferenceList<T>()
            .Select(item => new FacetItem
            {
                Code = def.PrimaryKey.GetValue(item)!.ToString()!,
                Label = (string)(def.DefaultProperty?.GetValue(item) ?? string.Empty),
                Count = 0,
            })
            .ToList();
    }

    /// <inheritdoc cref="IFacetDefinition{TDocument}.ResolveLabel" />
    public override string ResolveLabel(string primaryKey)
    {
        return referenceManager.GetReferenceValue<T>(primaryKey) ?? string.Empty;
    }
}
