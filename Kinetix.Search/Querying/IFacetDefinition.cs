using System.Linq.Expressions;

namespace Kinetix.Search.Querying;

/// <summary>
/// Définition d'une facette.
/// </summary>
public interface IFacetDefinition<TDocument>
{
    /// <summary>
    /// Code de la facette.
    /// </summary>
    string Code { get; }

    /// <summary>
    /// Libellé de la facette.
    /// </summary>
    string Label { get; }

    /// <summary>
    /// Champ sur lequel on facette.
    /// </summary>
    Expression<Func<TDocument, object>> Field { get; }

    /// <summary>
    /// Nom du champ sur lequel on facette.
    /// </summary>
    string FieldName { get; }

    /// <summary>
    /// Précise s'il est possible de sélectionner plusieurs valeurs en même temps sur la facette.
    /// </summary>
    bool IsMultiSelectable { get; }

    /// <summary>
    /// Précise i'il est possible d'exclure des valeurs de facette.
    /// </summary>
    bool CanExclude { get; }

    /// <summary>
    /// Précise si on doit ajouter une valeur spéciale "missing" quand la valeur de facette n'est pas renseignée.
    /// </summary>
    bool HasMissing { get; }

    /// <summary>
    /// Ordre des résultats dans la liste des valeurs.
    /// </summary>
    FacetOrdering Ordering { get; }

    /// <summary>
    /// Résout le libellé de la facette.
    /// </summary>
    /// <param name="primaryKey">Code ou libellé.</param>
    /// <returns>Le libellé de la facette.</returns>
    string ResolveLabel(string primaryKey);
}
