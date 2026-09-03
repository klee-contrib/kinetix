namespace Kinetix.DataAccess.Sql.Broker;

/// <summary>
/// Interface pour la persistence des données depuis
/// un broker.
/// </summary>
/// <typeparam name="T">Type du bean à manipuler.</typeparam>
public partial interface IStore<T>
    where T : new()
{
    /// <summary>
    /// Charge toutes les données pour un type.
    /// </summary>
    /// <param name="queryParameter">Paramètres de tri et de limite (vide par défaut).</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Collection.</returns>
    Task<IList<T>> LoadAllAsync(QueryParameter? queryParameter, CancellationToken ct = default);

    /// <summary>
    /// Récupération d'une liste d'objets d'un certain type correspondant à un critère donnée.
    /// </summary>
    /// <param name="criteria">Map de critères auquelle la recherche doit correpondre.</param>
    /// <param name="queryParameter">Paramètres de tri et de limite (vide par défaut).</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Collection.</returns>
    Task<IList<T>> LoadAllByCriteriaAsync(
        FilterCriteria criteria,
        QueryParameter? queryParameter,
        CancellationToken ct = default
    );

    /// <summary>
    /// Charge un bean à partir de sa clef primaire.
    /// </summary>
    /// <param name="primaryKey">Valeur de la clef primaire.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Bean.</returns>
    Task<T?> LoadAsync(object primaryKey, CancellationToken ct = default);

    /// <summary>
    /// Récupération d'un objet à partir de critères de recherches.
    /// </summary>
    /// <param name="criteria">Le critère de recherche.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Objet.</returns>
    Task<T?> LoadByCriteriaAsync(FilterCriteria criteria, CancellationToken ct = default);

    /// <summary>
    /// Dépose les beans dans le store.
    /// </summary>
    /// <param name="collection">Beans à enregistrer.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Beans enregistrés.</returns>
    Task<ICollection<T>> PutAllAsync(ICollection<T> collection, CancellationToken ct = default);

    /// <summary>
    /// Dépose un bean dans le store.
    /// </summary>
    /// <param name="bean">Bean à enregistrer.</param>
    /// <param name="forceInsert">Force un insert (au lieu de déterminer automatiquement en fonction de la PK).</param>
    /// <param name="columnSelector">Selecteur de colonnes à mettre à jour ou à ignorer.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Clef primaire de l'objet.</returns>
    Task<object> PutAsync(
        T bean,
        bool forceInsert,
        ColumnSelector? columnSelector = null,
        CancellationToken ct = default
    );

    /// <summary>
    /// Supprime tous les objets correspondant aux critères.
    /// </summary>
    /// <param name="criteria">Critères de suppression.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Task.</returns>
    Task RemoveAllByCriteriaAsync(FilterCriteria criteria, CancellationToken ct = default);

    /// <summary>
    /// Supprime un bean du store.
    /// </summary>
    /// <param name="primaryKey">Clef primaire du bean à supprimer.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>Task.</returns>
    Task RemoveAsync(object primaryKey, CancellationToken ct = default);
}
